using System;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Spawn;
using UnityEngine;

namespace Game.Arena.Infrastructure.Spawning
{
    /// <summary>
    /// Unity adapter behind <see cref="IEnemySpawnPlacementResolver"/>: chooses a
    /// Spawn Band point outside the current camera frustum, falling back to the
    /// band point farthest from the player's aim when the whole band is visible.
    /// The Domain remains the authority that accepts or rejects the point.
    /// </summary>
    public sealed class UnityEnemySpawnPlacementResolver : IEnemySpawnPlacementResolver
    {
        private const int OffsetsPerBand = 8;
        private const int PositionsPerSide = 8;
        private const int SideCount = 4;
        private const float MinimumDirectionLengthSquared = 1e-8f;

        private readonly ArenaDefinition _arena;
        private readonly Camera _camera;
        private readonly Transform _player;
        private readonly Plane[] _frustumPlanes;
        private readonly Vector3[] _candidates;

        public UnityEnemySpawnPlacementResolver(ArenaDefinition arena, Camera camera, Transform player)
        {
            _arena = arena ?? throw new ArgumentNullException(nameof(arena));
            _camera = camera;
            _player = player;
            _frustumPlanes = new Plane[6];
            _candidates = new Vector3[SideCount * OffsetsPerBand * PositionsPerSide];
        }

        public Position3D ResolveSpawnPosition(EnemySpawnRequest request)
        {
            float radius = request.CollisionRadius.Value.Value;
            int candidateCount = FillCandidates(radius);

            if (_camera != null)
            {
                GeometryUtility.CalculateFrustumPlanes(_camera, _frustumPlanes);

                for (int index = 0; index < candidateCount; index++)
                {
                    if (IsInsideView(_candidates[index], radius) == false)
                    {
                        return ToSpawnPosition(_candidates[index]);
                    }
                }
            }

            return ToSpawnPosition(FarthestFromAim(candidateCount));
        }

        /// <summary>
        /// Samples the Spawn Band ring deterministically: four sides, each with
        /// offsets outward from the arena edge and positions along the side. Every
        /// candidate sits at ground height, outside Arena Bounds, inside the band,
        /// and at least its collision radius away from the edge when the band allows.
        /// </summary>
        private int FillCandidates(float radius)
        {
            ArenaBounds bounds = _arena.Bounds;
            float band = _arena.SpawnBand.Value;
            float clearance = Mathf.Min(Mathf.Max(radius, 0f), band);
            float groundY = bounds.GroundY;
            int index = 0;

            for (int side = 0; side < SideCount; side++)
            {
                float alongStart = side < 2 ? bounds.MinimumZ : bounds.MinimumX;
                float alongEnd = side < 2 ? bounds.MaximumZ : bounds.MaximumX;

                for (int offsetIndex = 0; offsetIndex < OffsetsPerBand; offsetIndex++)
                {
                    float outward = clearance
                        + ((band - clearance) * offsetIndex / (OffsetsPerBand - 1));

                    for (int alongIndex = 0; alongIndex < PositionsPerSide; alongIndex++)
                    {
                        float along = alongStart
                            + ((alongEnd - alongStart) * alongIndex / (PositionsPerSide - 1));

                        _candidates[index++] = CreateCandidate(side, outward, along, groundY);
                    }
                }
            }

            return index;
        }

        private Vector3 CreateCandidate(int side, float outward, float along, float groundY)
        {
            ArenaBounds bounds = _arena.Bounds;

            switch (side)
            {
                case 0:
                    return new Vector3(bounds.MinimumX - outward, groundY, along);
                case 1:
                    return new Vector3(bounds.MaximumX + outward, groundY, along);
                case 2:
                    return new Vector3(along, groundY, bounds.MinimumZ - outward);
                default:
                    return new Vector3(along, groundY, bounds.MaximumZ + outward);
            }
        }

        private bool IsInsideView(Vector3 candidate, float radius)
        {
            Bounds body = new(
                candidate,
                new Vector3(radius * 2f, radius * 2f, radius * 2f));

            return GeometryUtility.TestPlanesAABB(_frustumPlanes, body);
        }

        private Vector3 FarthestFromAim(int candidateCount)
        {
            Vector3 aim = HorizontalAim();
            Vector3 origin = PlayerOrigin();
            Vector3 farthest = _candidates[0];
            float leastAlignment = float.PositiveInfinity;

            for (int index = 0; index < candidateCount; index++)
            {
                Vector3 offset = _candidates[index] - origin;
                offset.y = 0f;

                float alignment = offset.sqrMagnitude <= MinimumDirectionLengthSquared
                    ? 1f
                    : Vector3.Dot(offset.normalized, aim);

                if (alignment < leastAlignment)
                {
                    leastAlignment = alignment;
                    farthest = _candidates[index];
                }
            }

            return farthest;
        }

        private Vector3 HorizontalAim()
        {
            if (_player != null)
            {
                Vector3 forward = _player.forward;
                forward.y = 0f;

                if (forward.sqrMagnitude > MinimumDirectionLengthSquared)
                {
                    return forward.normalized;
                }
            }

            return Vector3.forward;
        }

        private Vector3 PlayerOrigin()
        {
            Vector3 origin = _player != null ? _player.position : Vector3.zero;
            origin.y = _arena.Bounds.GroundY;

            return origin;
        }

        private Position3D ToSpawnPosition(Vector3 candidate)
        {
            return new Position3D(candidate.x, _arena.Bounds.GroundY, candidate.z);
        }
    }
}
