using System;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Movement;
using UnityEngine;

namespace Game.Arena.Infrastructure.Movement
{
    public sealed class UnityPlayerMovementResolver : IPlayerMovementResolver
    {
        private const float MinimumTravelSquared = 1e-8f;
        private const float GroundNormalThreshold = 0.5f;
        private const float CollisionSkin = 1e-3f;

        private readonly Transform _player;
        private readonly int _layerMask;

        public UnityPlayerMovementResolver(Transform player, int layerMask = Physics.AllLayers)
        {
            _player = player != null ? player : throw new ArgumentNullException(nameof(player));
            _layerMask = layerMask;
        }

        public Position3D Resolve(PlayerMovementRequest request)
        {
            Vector3 from = new(request.From.X, 0f, request.From.Z);
            Vector3 to = new(request.RequestedPosition.X, 0f, request.RequestedPosition.Z);
            Vector3 displacement = to - from;

            if (displacement.sqrMagnitude <= MinimumTravelSquared)
            {
                return request.From;
            }

            float distance = displacement.magnitude;
            Vector3 direction = displacement / distance;
            float radius = request.CollisionRadius.Value.Value;
            Vector3 origin = new(from.x, _player.position.y + CenterHeightOffset(radius), from.z);

            float reachableDistance = FindReachableDistance(origin, direction, distance, radius);
            Vector3 accepted = from + (direction * reachableDistance);

            return new Position3D(accepted.x, request.RequestedPosition.Y, accepted.z);
        }

        private float CenterHeightOffset(float radius)
        {
            Collider playerCollider = _player.GetComponent<Collider>();

            return playerCollider != null
                ? playerCollider.bounds.center.y - _player.position.y
                : radius;
        }

        private float FindReachableDistance(
            Vector3 origin,
            Vector3 direction,
            float distance,
            float radius)
        {
            RaycastHit[] hits = Physics.SphereCastAll(
                origin,
                radius,
                direction,
                distance,
                _layerMask,
                QueryTriggerInteraction.Ignore);

            float reachableDistance = distance;

            for (int index = 0; index < hits.Length; index++)
            {
                RaycastHit hit = hits[index];

                if (hit.collider == null)
                {
                    continue;
                }

                if (hit.collider.transform.IsChildOf(_player))
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
    }
}
