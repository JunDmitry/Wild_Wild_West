using System.Collections.Generic;
using Game.Arena.Application.Input;
using Game.Arena.Application.Ports;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Infrastructure.Movement;
using Game.Arena.Infrastructure.Targeting;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class EnemyMovementTickTests
    {
        private const float ArenaHalfExtent = 20f;
        private const float GroundY = 0f;
        private const int PlayerHealthFull = 100;
        private const int PlayerHealthAfterOneHit = PlayerHealthFull - 5;

        // The Domain measures attack reach from both collision radii on top of
        // the Definition's AttackRange (ArenaRun.IsPlayerWithinEnemyAttackRange).
        private const float EnemyAttackRange = 2f;
        private const float EnemyRadius = 0.5f;
        private const float PlayerRadius = 0.5f;

        private readonly List<GameObject> _created = new();
        private TestEnemyTransformRegistry _transforms;
        private ArenaDefinition _arena;

        [SetUp]
        public void SetUp()
        {
            _arena = new ArenaDefinition(
                new ArenaBounds(
                    -ArenaHalfExtent,
                    ArenaHalfExtent,
                    -ArenaHalfExtent,
                    ArenaHalfExtent,
                    GroundY),
                Distance.FromValue(5f));

            _transforms = new TestEnemyTransformRegistry();
            _created.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _created.Count; index++)
            {
                UnityEngine.Object.DestroyImmediate(_created[index]);
            }

            _created.Clear();
        }

        [Test]
        public void EnemyClosesOnThePlayerEachTick()
        {
            PlayModeTickHarness harness = CreateHarness();
            EnemyId enemyId = SeedEnemy(harness, 1UL, new Position3D(21f, GroundY, 0f));

            ArenaRunSnapshot afterFirst = harness.TickIdle().Snapshot;
            ArenaRunSnapshot afterSecond = harness.TickIdle().Snapshot;

            Assert.That(PositionOf(afterFirst, enemyId).X, Is.EqualTo(20.8f).Within(0.001f));
            Assert.That(PositionOf(afterSecond, enemyId).X, Is.EqualTo(20.6f).Within(0.001f));
            Assert.That(PositionOf(afterSecond, enemyId).Y, Is.EqualTo(GroundY));
        }

        [Test]
        public void EnemyStopsAndAttacksOnceInRange()
        {
            Position3D playerPosition = new(17.5f, GroundY, 0f);
            PlayModeTickHarness harness = CreateHarness(playerPosition);
            EnemyId enemyId = SeedEnemy(harness, 1UL, new Position3D(21f, GroundY, 0f));

            ArenaRunSnapshot snapshot = harness.TickIdle().Snapshot;
            int ticks = 1;

            while (EnemyOf(snapshot, enemyId).HasPendingAttack == false && ticks < 40)
            {
                snapshot = harness.TickIdle().Snapshot;
                ticks++;
            }

            Assert.That(
                EnemyOf(snapshot, enemyId).HasPendingAttack,
                Is.True,
                "Enemy never started an attack after closing distance.");

            Position3D attackPosition = PositionOf(snapshot, enemyId);
            Assert.That(
                attackPosition.GroundDistanceTo(playerPosition),
                Is.LessThanOrEqualTo(EnemyAttackRange + EnemyRadius + PlayerRadius + 0.01f));

            int impactTicks = 0;

            while (snapshot.Player.Health.Current == PlayerHealthFull && impactTicks < 5)
            {
                snapshot = harness.TickIdle().Snapshot;
                impactTicks++;

                Assert.That(
                    PositionOf(snapshot, enemyId),
                    Is.EqualTo(attackPosition),
                    "Enemy moved while its attack was pending.");
            }

            Assert.That(snapshot.Player.Health.Current, Is.EqualTo(PlayerHealthAfterOneHit));
        }

        [Test]
        public void WallShortensTheStepAndTheDomainAcceptsIt()
        {
            Position3D playerPosition = new(17.5f, GroundY, 0f);
            PlayModeTickHarness harness = CreateHarness(playerPosition);
            EnemyId enemyId = SeedEnemy(harness, 1UL, new Position3D(21f, GroundY, 0f));
            CreateWall(new Vector3(20.3f, 0f, 0f));

            ArenaRunSnapshot snapshot = harness.TickIdle().Snapshot;

            float movedTo = PositionOf(snapshot, enemyId).X;

            Assert.That(movedTo, Is.GreaterThan(20.85f), "Wall did not shorten the step.");
            Assert.That(movedTo, Is.LessThan(20.95f));
            Assert.That(PositionOf(snapshot, enemyId).Z, Is.EqualTo(0f).Within(0.0001f));

            // A rejected resolution would leave the interaction pending and the
            // following tick would fail; a second clean tick proves acceptance.
            snapshot = harness.TickIdle().Snapshot;
            Assert.That(EnemyOf(snapshot, enemyId), Is.Not.Null);
            Assert.That(snapshot.Status, Is.EqualTo(ArenaRunStatus.Playing));
        }

        [Test]
        public void PlayerMovementChangesTheEnemyPursuitDirection()
        {
            SettableInputSource input = new();
            GameObject playerBody = CreateBody("Player", Vector3.zero);

            PlayModeTickHarness harness = CreateHarness(
                playerStart: null,
                playerInputSource: input,
                playerMovementResolver: new UnityPlayerMovementResolver(playerBody.transform));
            EnemyId enemyId = SeedEnemy(harness, 1UL, new Position3D(21f, GroundY, 0f));

            harness.TickFrame();

            input.Input = new PlayerFrameInput(
                new MovementAxes(1f, 0f),
                Direction3D.Forward,
                false,
                false);

            harness.TickFrame();
            ArenaRunSnapshot snapshot = harness.TickFrame().Snapshot;

            Assert.That(snapshot.Player.Position.Z, Is.GreaterThan(0.5f), "Player did not retreat sideways.");
            Assert.That(PositionOf(snapshot, enemyId).Z, Is.GreaterThan(0.001f));
            Assert.That(PositionOf(snapshot, enemyId).X, Is.LessThan(20.6f));
        }

        private PlayModeTickHarness CreateHarness(
            Position3D? playerStart = null,
            IPlayerInputSource playerInputSource = null,
            IPlayerMovementResolver playerMovementResolver = null)
        {
            return new PlayModeTickHarness(
                new EmptyEnemyColliderRegistry(),
                playerStart: playerStart,
                playerInputSource: playerInputSource,
                playerMovementResolver: playerMovementResolver,
                enemyMovementResolver: new UnityEnemyMovementResolver(_transforms, _arena));
        }

        private EnemyId SeedEnemy(PlayModeTickHarness harness, ulong value, Position3D position)
        {
            EnemyId enemyId = harness.SeedEnemy(value, position);
            Transform body = CreateBody(
                "Enemy " + value,
                new Vector3(position.X, position.Y, position.Z)).transform;

            _transforms.Register(enemyId, body);

            return enemyId;
        }

        private GameObject CreateBody(string name, Vector3 position)
        {
            GameObject body = new(name);
            body.transform.position = position;

            BoxCollider collider = body.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.5f, 0f);
            collider.size = new Vector3(1f, 1f, 1f);

            _created.Add(body);
            Physics.SyncTransforms();

            return body;
        }

        private void CreateWall(Vector3 position)
        {
            GameObject wall = new("Wall");
            wall.transform.position = position;

            BoxCollider collider = wall.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.5f, 0f);
            collider.size = new Vector3(0.2f, 1f, 4f);

            _created.Add(wall);
            Physics.SyncTransforms();
        }

        private static EnemySnapshot EnemyOf(ArenaRunSnapshot snapshot, EnemyId enemyId)
        {
            for (int index = 0; index < snapshot.Enemies.Count; index++)
            {
                if (snapshot.Enemies[index].EnemyId == enemyId)
                {
                    return snapshot.Enemies[index];
                }
            }

            return null;
        }

        private static Position3D PositionOf(ArenaRunSnapshot snapshot, EnemyId enemyId)
        {
            EnemySnapshot enemy = EnemyOf(snapshot, enemyId);

            if (enemy == null)
            {
                throw new System.InvalidOperationException("Enemy is not present in the snapshot: " + enemyId);
            }

            return enemy.Position;
        }

        private sealed class TestEnemyTransformRegistry : IEnemyTransformRegistry
        {
            private readonly Dictionary<EnemyId, Transform> _transforms = new();

            public void Register(EnemyId enemyId, Transform transform)
            {
                _transforms[enemyId] = transform;
            }

            public bool TryGetTransform(EnemyId enemyId, out Transform transform)
            {
                return _transforms.TryGetValue(enemyId, out transform);
            }
        }

        private sealed class EmptyEnemyColliderRegistry : IEnemyColliderRegistry
        {
            public bool TryGetEnemyId(Collider collider, out EnemyId enemyId)
            {
                enemyId = EnemyId.None;

                return false;
            }
        }

        private sealed class SettableInputSource : IPlayerInputSource
        {
            public PlayerFrameInput Input { get; set; } =
                new(MovementAxes.Zero, Direction3D.Forward, false, false);

            public PlayerFrameInput Read()
            {
                return Input;
            }
        }
    }
}
