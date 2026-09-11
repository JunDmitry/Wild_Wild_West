using System;
using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Simulation.Queries;
using Game.Core.Model.States;
using Game.Core.Rules.Mathematics;

namespace Game.Core.Rules.Planning
{
    public static class SimulationPlanner
    {
        private const float Epsilon = .0001f;

        public static SimulationPlan Plan(
            in GameState state,
            in GameConfig config,
            in FrameInput input,
            float delta)
        {
            if (state.Phase != Model.Enums.GamePhase.Playing)
            {
                return SimulationPlan.Empty;
            }

            bool hasMovement = TryPlanMovement(
                state.Player,
                input.MoveDirection,
                config.Player.MoveSpeed,
                config.Arena,
                delta,
                out MovementQuery movementQuery);

            WeaponKind activeWeapon = state.Player.SelectedWeapon;

            if (input.SwitchWeaponPressed)
            {
                activeWeapon = activeWeapon == WeaponKind.Ranged ? WeaponKind.Melee : WeaponKind.Ranged;
            }

            AttackQuery attackQuery = PlanAttack(
                state.Player,
                activeWeapon,
                input,
                config,
                state.Time + delta);
            SpawnQuery spawnQuery = PlanSpawn(state.CurrentWave, config);

            return new(
                hasMovement,
                movementQuery,
                attackQuery,
                spawnQuery);
        }

        private static bool TryPlanMovement(
            in PlayerState player,
            Direction3D moveDirection,
            float moveSpeed,
            in ArenaConfig arena,
            float delta,
            out MovementQuery movementQuery)
        {
            float length = Math3D.LengthXZ(moveDirection);

            if (length <= Epsilon)
            {
                movementQuery = default;
                return false;
            }

            Direction3D direction = Math3D.NormalizeXZ(moveDirection);
            float scale = Math3D.Clamp01(length);
            float distanceDelta = moveSpeed * scale * delta;

            Position3D candidate = Math3D.OffsetXZ(player.Position, direction, distanceDelta);
            candidate = MovementRules.ClampToArena(candidate, arena);

            movementQuery = new(player.Position, candidate, arena.PlayerRadius);

            return true;
        }

        private static AttackQuery PlanAttack(
            in PlayerState player,
            WeaponKind activeWeapon,
            in FrameInput input,
            in GameConfig config,
            float targetTime)
        {
            if (input.AttackPressed == false)
            {
                return AttackQuery.None;
            }

            WeaponConfig weaponConfig;
            float readyTime;

            if (activeWeapon == WeaponKind.Ranged)
            {
                weaponConfig = config.RangedWeapon;
                readyTime = player.RangedReadyTime;
            }
            else
            {
                weaponConfig = config.MeleeWeapon;
                readyTime = player.MeleeReadyTime;
            }

            if (targetTime < readyTime)
            {
                return AttackQuery.None;
            }

            if (activeWeapon == WeaponKind.Ranged)
            {
                return AttackQuery.Ranged(input.AimOrigin, input.AimDirection, weaponConfig.Range);
            }

            return AttackQuery.Melee(player.Position, weaponConfig.Range);
        }

        private static SpawnQuery PlanSpawn(
            in WaveState currentWave,
            in GameConfig config)
        {
            if (currentWave.Phase == WavePhase.RegularCombat && currentWave.RegularToSpawn > 0)
            {
                return SpawnQuery.Regular;
            }

            if (currentWave.Phase == WavePhase.BossCombat && currentWave.BossStatus == BossStatus.NotSpawned)
            {
                return SpawnQuery.Boss;
            }

            return SpawnQuery.None;
        }
    }
}
