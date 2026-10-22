using Game.Arena.Application.Input;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Ticks
{
    [TestFixture]
    public sealed class DefeatRestartTickTests
    {
        private const double RestartDelaySeconds = 0.3d;

        private TickCoordinatorTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new TickCoordinatorTestKit(RestartDelaySeconds);
            _kit.Session.StartInitialRun();
            _kit.Input.Input = new PlayerFrameInput(
                MovementAxes.Zero,
                Direction3D.Right,
                false,
                false);
        }

        [Test]
        public void DefeatedRunIsNotRestartedBeforeTheDelay()
        {
            ArenaRun defeated = DefeatActiveRun();

            _kit.Coordinator.ExecuteTick();
            ArenaRunTickResult result = _kit.Coordinator.ExecuteTick();

            Assert.That(result.ArenaRunId, Is.EqualTo(defeated.Id));
            Assert.That(result.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Defeat));
            Assert.That(result.ExecutedStages, Does.Contain(ArenaRunTickStage.DefeatRestart));
            Assert.That(_kit.Session.ActiveArenaRunId, Is.EqualTo(defeated.Id));
        }

        [Test]
        public void DefeatedRunRestartsWhenTheDelayElapses()
        {
            ArenaRun defeated = DefeatActiveRun();

            ArenaRunTickResult result = ExecuteTicksUntilRestarted();

            Assert.That(result.ArenaRunId, Is.Not.EqualTo(defeated.Id));
            Assert.That(result.ArenaRunId, Is.EqualTo(_kit.Session.ActiveArenaRunId));
            Assert.That(result.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Playing));
            Assert.That(result.FinalRevision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(result.ExecutedStages, Does.Contain(ArenaRunTickStage.DefeatRestart));
        }

        [Test]
        public void RestartedRunBeginsAtTheFirstWaveWithFullHealthAndFirearm()
        {
            DefeatActiveRun();

            ArenaRunSnapshot snapshot = ExecuteTicksUntilRestarted().Snapshot;

            Assert.That(snapshot.Wave.Number, Is.EqualTo(WaveNumber.First));
            Assert.That(snapshot.Player.Health.Current, Is.EqualTo(snapshot.Player.Health.Maximum));
            Assert.That(snapshot.Player.SelectedWeapon, Is.EqualTo(WeaponKind.Ranged));
        }

        [Test]
        public void RestartDiscardsEverythingFromTheDefeatedRun()
        {
            ArenaRun defeated = DefeatActiveRun();

            ArenaRunSnapshot defeatedSnapshot =
                _kit.SnapshotMapper.Map(defeated.CreateSnapshot());

            Assert.That(defeatedSnapshot.Enemies, Is.Not.Empty);

            ArenaRunSnapshot snapshot = ExecuteTicksUntilRestarted().Snapshot;

            Assert.That(snapshot.Enemies, Is.Empty);
            Assert.That(snapshot.CurrentTime, Is.EqualTo(new GameTimePoint(0d)));
            Assert.That(snapshot.Wave.Number, Is.EqualTo(WaveNumber.First));
            Assert.That(
                snapshot.Wave.RegularEnemiesRemainingToSpawn,
                Is.EqualTo(10));
            Assert.That(snapshot.Wave.BossStatus, Is.EqualTo(BossStatus.NotSpawned));
        }

        [Test]
        public void RestartReplacesTheDefeatedRunInTheRepository()
        {
            ArenaRun defeated = DefeatActiveRun();

            ExecuteTicksUntilRestarted();

            Assert.That(_kit.Repository.TryGet(defeated.Id, out _), Is.False);

            Assert.That(
                _kit.Repository.TryGet(_kit.Session.ActiveArenaRunId, out ArenaRun restarted),
                Is.True);
            Assert.That(restarted.Status, Is.EqualTo(ArenaRunStatus.Playing));
        }

        [Test]
        public void PlayingRunIsNeverRestarted()
        {
            ArenaRun run = _kit.Session.GetRequiredActiveRun();

            for (int tick = 0; tick < 10; tick++)
            {
                _kit.Coordinator.ExecuteTick();
            }

            Assert.That(_kit.Session.ActiveArenaRunId, Is.EqualTo(run.Id));
            Assert.That(run.Status, Is.EqualTo(ArenaRunStatus.Playing));
        }

        private ArenaRun DefeatActiveRun()
        {
            ArenaRun run = _kit.Session.GetRequiredActiveRun();
            run.MakeRunDefeat(reqularsToSpawn: 1);

            return run;
        }

        private ArenaRunTickResult ExecuteTicksUntilRestarted()
        {
            ArenaRunTickResult result = _kit.Coordinator.ExecuteTick();

            for (int tick = 0; tick < 10 && result.Snapshot.Status == ArenaRunStatus.Defeat; tick++)
            {
                result = _kit.Coordinator.ExecuteTick();
            }

            return result;
        }
    }
}
