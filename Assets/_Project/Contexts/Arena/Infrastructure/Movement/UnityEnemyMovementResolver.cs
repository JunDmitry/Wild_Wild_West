using System;
using System.Collections.Generic;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Movement;
using UnityEngine;

namespace Game.Arena.Infrastructure.Movement
{
    /// <summary>
    /// Unity adapter behind <see cref="IEnemyMovementResolver"/>: accepts a
    /// position along each Domain pursuit intent, shortened only by Arena Bounds
    /// and scene geometry. The Domain remains the authority that validates the
    /// movement path and applies the step, so this resolver never re-aims,
    /// extends, adds, or drops an intent.
    /// </summary>
    public sealed class UnityEnemyMovementResolver : IEnemyMovementResolver
    {
        private const int BufferSize = 64;
        private const float MinimumDirectionLengthSquared = 1e-8f;
        private const float GroundNormalThreshold = 0.5f;
        private const float CollisionSkin = 1e-3f;

        private readonly IEnemyTransformRegistry _enemyTransforms;
        private readonly ArenaDefinition _arena;
        private readonly int _layerMask;
        private readonly RaycastHit[] _hitsBuffer;
        private readonly List<EnemyMovementBatchResolutionEntry> _entries = new();

        public UnityEnemyMovementResolver(
            IEnemyTransformRegistry enemyTransforms,
            ArenaDefinition arena,
            int layerMask = Physics.AllLayers)
        {
            _enemyTransforms = enemyTransforms ?? throw new ArgumentNullException(nameof(enemyTransforms));
            _arena = arena ?? throw new ArgumentNullException(nameof(arena));
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

            for (int index = 0; index < intents.Count; index++)
            {
                _entries.Add(ResolveIntent(intents[index]));
            }

            return _entries;
        }

        private EnemyMovementBatchResolutionEntry ResolveIntent(EnemyMovementIntent intent)
        {
            PlanarMovementIntent movement = intent.Intent;
            float requested = movement.RequestedDistance.Value;
            Vector3 direction = new(movement.Direction.X, 0f, movement.Direction.Z);

            if (direction.sqrMagnitude <= MinimumDirectionLengthSquared || requested <= 0f)
            {
                return new EnemyMovementBatchResolutionEntry(intent.EnemyId, movement.From);
            }

            direction.Normalize();

            Vector3 from = new(movement.From.X, movement.From.Y, movement.From.Z);
            float radius = intent.CollisionRadius.Value.Value;

            float travel = Mathf.Min(requested, BoundsLimitedDistance(from, direction, radius));
            travel = Mathf.Min(
                travel,
                GeometryLimitedDistance(intent.EnemyId, from, direction, travel, radius));

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
            float distance,
            float radius)
        {
            bool hasTransform = _enemyTransforms.TryGetTransform(enemyId, out Transform enemyTransform)
                && enemyTransform != null;

            float centerHeight = hasTransform
                ? CenterHeightOffset(enemyTransform, radius)
                : radius;

            Vector3 origin = new(from.x, from.y + centerHeight, from.z);

            int hitsCount = Physics.SphereCastNonAlloc(
                origin,
                radius,
                direction,
                _hitsBuffer,
                distance,
                _layerMask,
                QueryTriggerInteraction.Ignore);

            float reachableDistance = distance;

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
                    continue;
                }

                if (hit.normal.y > GroundNormalThreshold)
                {
                    continue;
                }

                if (hit.distance >= reachableDistance)
                {
                    continue;
                }

                reachableDistance = hit.distance;
            }

            if (reachableDistance >= distance)
            {
                return distance;
            }

            return Mathf.Max(0f, reachableDistance - CollisionSkin);
        }

        private static float CenterHeightOffset(Transform enemyTransform, float radius)
        {
            Collider enemyCollider = enemyTransform.GetComponent<Collider>();

            return enemyCollider != null
                ? enemyCollider.bounds.center.y - enemyTransform.position.y
                : radius;
        }
    }
}
