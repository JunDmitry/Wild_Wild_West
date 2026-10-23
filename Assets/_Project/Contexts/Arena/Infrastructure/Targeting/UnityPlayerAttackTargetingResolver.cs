using System;
using System.Collections.Generic;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Attack;
using UnityEngine;

namespace Game.Arena.Infrastructure.Targeting
{
    public sealed class UnityPlayerAttackTargetingResolver : IPlayerAttackTargetingResolver
    {
        private const int BufferSize = 64;
        private const float MinimumDirectionLengthSquared = 1e-8f;
        private const float MinimumOffsetLengthSquared = 1e-8f;

        private static readonly IReadOnlyList<EnemyId> s_noTargets = Array.Empty<EnemyId>();

        private readonly IEnemyColliderRegistry _enemyColliders;
        private readonly RaycastHit[] _raycastHitsBuffer;
        private readonly Collider[] _collidersBuffer;

        public UnityPlayerAttackTargetingResolver(IEnemyColliderRegistry enemyColliders)
        {
            _enemyColliders = enemyColliders ?? throw new ArgumentNullException(nameof(enemyColliders));
            _raycastHitsBuffer = new RaycastHit[BufferSize];
            _collidersBuffer = new Collider[BufferSize];
        }

        public IReadOnlyList<EnemyId> Resolve(PlayerAttackImpactRequest request)
        {
            Vector3 origin = new(request.Origin.X, request.Origin.Y, request.Origin.Z);
            Vector3 direction = new(request.AimDirection.X, 0f, request.AimDirection.Z);

            if (direction.sqrMagnitude <= MinimumDirectionLengthSquared)
            {
                return s_noTargets;
            }

            direction.Normalize();

            return request.WeaponKind == WeaponKind.Melee
                ? ResolveMelee(request, origin, direction)
                : ResolveRanged(request, origin, direction);
        }

        private IReadOnlyList<EnemyId> ResolveRanged(
            PlayerAttackImpactRequest request,
            Vector3 origin,
            Vector3 direction)
        {
            int hitsCount = Physics.RaycastNonAlloc(
                origin,
                direction,
                _raycastHitsBuffer,
                request.Range.Value,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);

            EnemyId nearestEnemy = EnemyId.None;
            float nearestDistance = float.PositiveInfinity;

            for (int index = 0; index < hitsCount; index++)
            {
                if (_enemyColliders.TryGetEnemyId(_raycastHitsBuffer[index].collider, out EnemyId enemyId) == false)
                {
                    continue;
                }

                if (_raycastHitsBuffer[index].distance >= nearestDistance)
                {
                    continue;
                }

                nearestDistance = _raycastHitsBuffer[index].distance;
                nearestEnemy = enemyId;
            }

            if (nearestEnemy.IsNone)
            {
                return s_noTargets;
            }

            return new[] { nearestEnemy };
        }

        private IReadOnlyList<EnemyId> ResolveMelee(
            PlayerAttackImpactRequest request,
            Vector3 origin,
            Vector3 direction)
        {
            int overlapsCount = Physics.OverlapSphereNonAlloc(
                origin,
                request.Range.Value,
                _collidersBuffer,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);

            float rangeSquared = request.Range.Value * request.Range.Value;
            List<EnemyId> targets = new();
            HashSet<EnemyId> seen = new();

            for (int index = 0; index < overlapsCount; index++)
            {
                if (_enemyColliders.TryGetEnemyId(_collidersBuffer[index], out EnemyId enemyId) == false)
                {
                    continue;
                }

                if (seen.Contains(enemyId))
                {
                    continue;
                }

                Vector3 offset = _collidersBuffer[index].transform.position - origin;
                offset.y = 0f;

                float offsetSquared = offset.sqrMagnitude;

                if (offsetSquared > rangeSquared)
                {
                    continue;
                }

                if (request.Arc.IsFull == false
                    && offsetSquared > MinimumOffsetLengthSquared
                    && Vector3.Dot(offset / Mathf.Sqrt(offsetSquared), direction) < request.Arc.CosHalfAngle)
                {
                    continue;
                }

                seen.Add(enemyId);
                targets.Add(enemyId);
            }

            return targets.Count == 0 ? s_noTargets : targets;
        }
    }
}
