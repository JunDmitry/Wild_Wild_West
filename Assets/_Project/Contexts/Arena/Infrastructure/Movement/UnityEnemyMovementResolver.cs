using System;
using System.Collections.Generic;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Movement;
using Game.Arena.Infrastructure.Targeting;
using UnityEngine;

namespace Game.Arena.Infrastructure.Movement
{
    /// <summary>
    /// Unity adapter behind <see cref="IEnemyMovementResolver"/>: accepts a
    /// position along each Domain pursuit intent, shortened only by Arena Bounds
    /// and scene geometry. On each resolve it spends at most
    /// <see cref="QueryBudget"/> scene-geometry queries, picking enemies by age
    /// priority (never queried, then oldest, then a materially changed
    /// direction). A queried enemy casts far enough to clear the whole window in
    /// which it may be skipped, and a skipped enemy reuses the world-space
    /// obstruction point from that query: the distance to it is recomputed from
    /// the current position, so advancing toward the obstruction shrinks the
    /// allowed step instead of letting the enemy pass through it (ADR-0038). The
    /// Domain remains the authority that validates the movement path and applies
    /// the step, so this resolver never re-aims, extends, adds, or drops an
    /// intent.
    /// </summary>
    public sealed class UnityEnemyMovementResolver : IEnemyMovementResolver
    {
        private const int BufferSize = 64;
        private const int QueryBudget = 3;
        private const int InitialCacheCapacity = 64;
        private const float MinimumDirectionLengthSquared = 1e-8f;
        private const float GroundNormalThreshold = 0.5f;
        private const float CollisionSkin = 1e-3f;

        // Dot product below which a pursuit direction counts as materially
        // changed when breaking an equal-age tie between cached results.
        private const float MaterialDirectionChangeDot = 0.999f;

        private readonly IEnemyTransformRegistry _enemyTransforms;
        private readonly IEnemyColliderRegistry _enemyColliders;
        private readonly ArenaDefinition _arena;
        private readonly int _layerMask;
        private readonly RaycastHit[] _hitsBuffer;
        private readonly List<EnemyMovementBatchResolutionEntry> _entries = new();
        private readonly Dictionary<EnemyId, GeometryCacheEntry> _geometryCache = new(InitialCacheCapacity);
        private readonly List<EnemyId> _pruneBuffer = new(InitialCacheCapacity);
        private bool[] _selected = new bool[InitialCacheCapacity];
        private float[] _movementCaps = new float[InitialCacheCapacity];
        private int _selectionCount;
        private long _tick;

        public UnityEnemyMovementResolver(
            IEnemyTransformRegistry enemyTransforms,
            ArenaDefinition arena,
            IEnemyColliderRegistry enemyColliders,
            int layerMask = Physics.AllLayers)
        {
            _enemyTransforms = enemyTransforms ?? throw new ArgumentNullException(nameof(enemyTransforms));
            _arena = arena ?? throw new ArgumentNullException(nameof(arena));
            _enemyColliders = enemyColliders ?? throw new ArgumentNullException(nameof(enemyColliders));
            _layerMask = layerMask;
            _hitsBuffer = new RaycastHit[BufferSize];
        }

        public IReadOnlyList<EnemyMovementBatchResolutionEntry> Resolve(EnemyMovementBatchRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            IReadOnlyList<EnemyMovementIntent> intents = request.Intents;
            _entries.Clear();
            _tick++;
            SelectQueries(intents);
            int horizon = StalenessHorizon(intents.Count);

            for (int index = 0; index < intents.Count; index++)
            {
                _entries.Add(ResolveIntent(intents[index], _selected[index], horizon));
            }

            ApplySimultaneousMovementLimit(intents);
            PruneAbsentEntries();

            return _entries;
        }

        /// <summary>
        /// Number of ticks an intent may wait between queries at the current
        /// batch size. A queried intent casts far enough to stay safe for this
        /// whole window, so a reused result is never shorter-lived than the gap
        /// it has to cover.
        /// </summary>
        private static int StalenessHorizon(int intentCount)
        {
            int horizon = (intentCount + QueryBudget - 1) / QueryBudget;
            return horizon < 1 ? 1 : horizon;
        }

        /// <summary>
        /// Marks up to <see cref="QueryBudget"/> intents for a fresh geometry
        /// query: never-queried enemies first, then the enemies whose cached
        /// result is oldest, with a materially changed direction breaking an
        /// age tie and the stable batch index breaking the rest.
        /// </summary>
        private void SelectQueries(IReadOnlyList<EnemyMovementIntent> intents)
        {
            int count = intents.Count;
            PrepareSelection(count);
            _selectionCount = 0;

            for (int index = 0; index < count && _selectionCount < QueryBudget; index++)
            {
                if (_geometryCache.ContainsKey(intents[index].EnemyId))
                {
                    continue;
                }

                _selected[index] = true;
                _selectionCount++;
            }

            while (_selectionCount < QueryBudget)
            {
                int best = -1;
                GeometryCacheEntry bestEntry = default;
                bool bestChanged = false;

                for (int index = 0; index < count; index++)
                {
                    if (_selected[index]
                        || !_geometryCache.TryGetValue(intents[index].EnemyId, out GeometryCacheEntry entry))
                    {
                        continue;
                    }

                    bool changed = HasMateriallyChangedDirection(intents[index], entry);

                    if (best < 0
                        || entry.LastQueryTick < bestEntry.LastQueryTick
                        || (entry.LastQueryTick == bestEntry.LastQueryTick && changed && !bestChanged))
                    {
                        best = index;
                        bestEntry = entry;
                        bestChanged = changed;
                    }
                }

                if (best < 0)
                {
                    break;
                }

                _selected[best] = true;
                _selectionCount++;
            }
        }

        private void PrepareSelection(int count)
        {
            if (_selected.Length < count)
            {
                _selected = new bool[count];
                return;
            }

            Array.Clear(_selected, 0, count);
        }

        private static bool HasMateriallyChangedDirection(
            EnemyMovementIntent intent,
            in GeometryCacheEntry entry)
        {
            PlanarMovementIntent movement = intent.Intent;
            float x = movement.Direction.X;
            float z = movement.Direction.Z;
            float lengthSquared = (x * x) + (z * z);

            if (lengthSquared <= MinimumDirectionLengthSquared)
            {
                return true;
            }

            float length = Mathf.Sqrt(lengthSquared);
            float dot = ((x / length) * entry.DirectionX) + ((z / length) * entry.DirectionZ);

            return dot < MaterialDirectionChangeDot;
        }

        private void PruneAbsentEntries()
        {
            _pruneBuffer.Clear();

            foreach (KeyValuePair<EnemyId, GeometryCacheEntry> pair in _geometryCache)
            {
                if (pair.Value.LastSeenTick != _tick)
                {
                    _pruneBuffer.Add(pair.Key);
                }
            }

            for (int index = 0; index < _pruneBuffer.Count; index++)
            {
                _geometryCache.Remove(_pruneBuffer[index]);
            }
        }

        private void StoreGeometry(
            EnemyId enemyId,
            Vector3 from,
            Vector3 direction,
            float contactDistance,
            bool obstructed)
        {
            GeometryCacheEntry entry = _geometryCache.TryGetValue(
                enemyId,
                out GeometryCacheEntry cached)
                ? cached
                : default;

            entry.ObstructionX = from.x + (direction.x * contactDistance);
            entry.ObstructionZ = from.z + (direction.z * contactDistance);
            entry.Obstructed = obstructed;
            entry.DirectionX = direction.x;
            entry.DirectionZ = direction.z;
            entry.LastQueryTick = _tick;
            entry.LastSeenTick = _tick;
            _geometryCache[enemyId] = entry;
        }

        /// <summary>
        /// Distance a skipped enemy may still travel along its current direction
        /// before reaching the obstruction it last queried. The cached point is
        /// in world space, so the Domain advancing the enemy toward it shrinks
        /// the result on every tick; a point behind the current direction no
        /// longer constrains the step.
        /// </summary>
        private static float ReusedDistance(
            in GeometryCacheEntry entry,
            Vector3 from,
            Vector3 direction)
        {
            float offsetX = entry.ObstructionX - from.x;
            float offsetZ = entry.ObstructionZ - from.z;
            float along = (offsetX * direction.x) + (offsetZ * direction.z);

            if (along <= 0f)
            {
                return float.PositiveInfinity;
            }

            return entry.Obstructed
                ? Mathf.Max(0f, along - CollisionSkin)
                : along;
        }

        private EnemyMovementBatchResolutionEntry ResolveIntent(
            EnemyMovementIntent intent,
            bool queryGeometry,
            int horizon)
        {
            PlanarMovementIntent movement = intent.Intent;
            float requested = movement.RequestedDistance.Value;
            Vector3 direction = new(movement.Direction.X, 0f, movement.Direction.Z);

            bool hasCached = _geometryCache.TryGetValue(intent.EnemyId, out GeometryCacheEntry cached);

            if (hasCached)
            {
                cached.LastSeenTick = _tick;
                _geometryCache[intent.EnemyId] = cached;
            }

            if (direction.sqrMagnitude <= MinimumDirectionLengthSquared || requested <= 0f)
            {
                return new EnemyMovementBatchResolutionEntry(intent.EnemyId, movement.From);
            }

            direction.Normalize();

            Vector3 from = new(movement.From.X, movement.From.Y, movement.From.Z);
            float radius = intent.CollisionRadius.Value.Value;

            float travel = Mathf.Min(requested, BoundsLimitedDistance(from, direction, radius));

            float geometryLimited;

            if (queryGeometry)
            {
                geometryLimited = GeometryLimitedDistance(
                    intent.EnemyId,
                    from,
                    direction,
                    travel,
                    radius,
                    travel * horizon,
                    out float contactDistance,
                    out bool obstructed);
                StoreGeometry(intent.EnemyId, from, direction, contactDistance, obstructed);
            }
            else
            {
                geometryLimited = hasCached
                    ? ReusedDistance(cached, from, direction)
                    : float.PositiveInfinity;
            }

            travel = Mathf.Min(travel, geometryLimited);

            if (travel <= 0f)
            {
                return new EnemyMovementBatchResolutionEntry(intent.EnemyId, movement.From);
            }

            Position3D accepted = travel >= requested
                ? movement.RequestedPosition
                : movement.From.MovedAlong(movement.Direction, Distance.FromValue(travel));

            return new EnemyMovementBatchResolutionEntry(intent.EnemyId, accepted);
        }

        /// <summary>
        /// Shortens each intent so that no pair of enemies moving in the same
        /// batch ends closer than the sum of their collision radii. The physics
        /// cast only sees the other enemy's starting collider and each intent's
        /// own requested travel, so two enemies closing from opposite sides can
        /// each find a clear path and meet (or cross) inside one tick. This pass
        /// caps both at the first contact fraction of their relative motion.
        /// It is conservative: a pair that already overlaps is left where it is
        /// rather than pushed apart (ADR-0039: no separation push).
        /// </summary>
        private void ApplySimultaneousMovementLimit(IReadOnlyList<EnemyMovementIntent> intents)
        {
            int count = intents.Count;
            PrepareMovementCaps(count);

            for (int first = 0; first < count; first++)
            {
                Position3D firstStart = intents[first].Intent.From;
                Position3D firstEnd = _entries[first].AcceptedPosition;
                float firstRadius = intents[first].CollisionRadius.Value.Value;

                for (int second = first + 1; second < count; second++)
                {
                    Position3D secondStart = intents[second].Intent.From;
                    Position3D secondEnd = _entries[second].AcceptedPosition;
                    float secondRadius = intents[second].CollisionRadius.Value.Value;

                    float cap = ContactFraction(
                        firstStart,
                        firstEnd,
                        secondStart,
                        secondEnd,
                        firstRadius + secondRadius);

                    if (cap < _movementCaps[first])
                    {
                        _movementCaps[first] = cap;
                    }

                    if (cap < _movementCaps[second])
                    {
                        _movementCaps[second] = cap;
                    }
                }
            }

            for (int index = 0; index < count; index++)
            {
                float cap = _movementCaps[index];

                if (cap >= 1f)
                {
                    continue;
                }

                Position3D start = intents[index].Intent.From;
                Position3D end = _entries[index].AcceptedPosition;
                float x = start.X + ((end.X - start.X) * cap);
                float z = start.Z + ((end.Z - start.Z) * cap);

                _entries[index] = new EnemyMovementBatchResolutionEntry(
                    _entries[index].EnemyId,
                    new Position3D(x, end.Y, z));
            }
        }

        private void PrepareMovementCaps(int count)
        {
            if (_movementCaps.Length < count)
            {
                _movementCaps = new float[count];
            }

            for (int index = 0; index < count; index++)
            {
                _movementCaps[index] = 1f;
            }
        }

        /// <summary>
        /// Earliest fraction of the tick at which two points moving linearly
        /// from their start to their end would be closer than
        /// <paramref name="minimumDistance"/>; <c>1</c> when they never are.
        /// With <c>p</c> the start separation and <c>v</c> the relative
        /// displacement, the separation squared is
        /// <c>v²t² + 2(p·v)t + p²</c>.
        /// </summary>
        private static float ContactFraction(
            Position3D firstStart,
            Position3D firstEnd,
            Position3D secondStart,
            Position3D secondEnd,
            float minimumDistance)
        {
            float px = firstStart.X - secondStart.X;
            float pz = firstStart.Z - secondStart.Z;
            float vx = (firstEnd.X - firstStart.X) - (secondEnd.X - secondStart.X);
            float vz = (firstEnd.Z - firstStart.Z) - (secondEnd.Z - secondStart.Z);

            float minimumSquared = minimumDistance * minimumDistance;
            float startSquared = (px * px) + (pz * pz);

            if (startSquared < minimumSquared)
            {
                return 0f;
            }

            float speedSquared = (vx * vx) + (vz * vz);

            if (speedSquared <= MinimumDirectionLengthSquared)
            {
                return 1f;
            }

            float relativeDot = (px * vx) + (pz * vz);

            if (relativeDot >= 0f)
            {
                return 1f;
            }

            float discriminant = (relativeDot * relativeDot)
                - (speedSquared * (startSquared - minimumSquared));

            if (discriminant <= 0f)
            {
                return 1f;
            }

            float entry = (-relativeDot - Mathf.Sqrt(discriminant)) / speedSquared;

            if (entry <= 0f)
            {
                return 0f;
            }

            return entry < 1f ? entry : 1f;
        }

        /// <summary>
        /// Distance the Enemy may travel before its collision radius would leave
        /// Arena Bounds. An Enemy already outside the Bounds (a fresh spawn) is
        /// free to move inward; only the boundary it is heading toward limits it.
        /// </summary>
        private float BoundsLimitedDistance(Vector3 from, Vector3 direction, float radius)
        {
            ArenaBounds bounds = _arena.Bounds;

            float availableX = AvailableAlongAxis(
                from.x,
                direction.x,
                bounds.MinimumX + radius,
                bounds.MaximumX - radius);

            float availableZ = AvailableAlongAxis(
                from.z,
                direction.z,
                bounds.MinimumZ + radius,
                bounds.MaximumZ - radius);

            return Mathf.Min(availableX, availableZ);
        }

        private static float AvailableAlongAxis(
            float position,
            float direction,
            float minimum,
            float maximum)
        {
            if (direction > 0f)
            {
                return Mathf.Max(0f, (maximum - position) / direction);
            }

            if (direction < 0f)
            {
                return Mathf.Max(0f, (minimum - position) / direction);
            }

            return float.PositiveInfinity;
        }

        private float GeometryLimitedDistance(
            EnemyId enemyId,
            Vector3 from,
            Vector3 direction,
            float travel,
            float radius,
            float castDistance,
            out float contactDistance,
            out bool obstructed)
        {
            bool hasTransform = _enemyTransforms.TryGetTransform(enemyId, out Transform enemyTransform)
                && enemyTransform != null;

            bool hasCollider = _enemyTransforms.TryGetCollider(enemyId, out Collider enemyCollider)
                && enemyCollider != null;

            float centerHeight = hasCollider
                ? enemyCollider.bounds.center.y
                : from.y + radius;

            Vector3 origin = new(from.x, centerHeight, from.z);

            int hitsCount = Physics.SphereCastNonAlloc(
                origin,
                radius,
                direction,
                _hitsBuffer,
                castDistance,
                _layerMask,
                QueryTriggerInteraction.Ignore);

            float nearest = castDistance;

            for (int index = 0; index < hitsCount; index++)
            {
                RaycastHit hit = _hitsBuffer[index];

                if (hit.collider == null)
                {
                    continue;
                }

                if (hasTransform && hit.collider.transform.IsChildOf(enemyTransform))
                {
                    continue;
                }

                if (hit.distance <= 0f)
                {
                    if (_enemyColliders.TryGetEnemyId(hit.collider, out EnemyId hitEnemyId)
                        && hitEnemyId != enemyId)
                    {
                        nearest = 0f;
                        break;
                    }

                    continue;
                }

                if (hit.normal.y > GroundNormalThreshold)
                {
                    continue;
                }

                if (hit.distance >= nearest)
                {
                    continue;
                }

                nearest = hit.distance;
            }

            contactDistance = nearest;
            obstructed = nearest < castDistance;

            if (!obstructed)
            {
                return travel;
            }

            return Mathf.Min(travel, Mathf.Max(0f, nearest - CollisionSkin));
        }

        private struct GeometryCacheEntry
        {
            // World-space point of the nearest obstruction along the queried
            // direction, or the far end of the cast when the path was clear.
            public float ObstructionX;
            public float ObstructionZ;
            public bool Obstructed;
            public float DirectionX;
            public float DirectionZ;
            public long LastQueryTick;
            public long LastSeenTick;
        }
    }
}
