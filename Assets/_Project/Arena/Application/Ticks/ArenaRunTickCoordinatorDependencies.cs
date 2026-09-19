using Game.Arena.Application.Ports;
using Game.Arena.Application.Sessions;
using Game.Arena.Application.Ticks.Stages;

namespace Game.Arena.Application.Ticks
{
    internal sealed class ArenaRunTickCoordinatorDependencies
    {
        public ArenaRunTickCoordinatorDependencies(
            ArenaRunSession session,
            IGameClock clock,
            IPlayerInputSource inputSource,
            PlayerTickPhase playerPhase,
            EnemyTickPhase enemyPhase,
            PendingInteractionRecoveryStage recoveryStage,
            TimeAdvanceStage timeAdvanceStage)
        {
            Session = session ?? throw new System.ArgumentNullException(nameof(session));
            Clock = clock ?? throw new System.ArgumentNullException(nameof(clock));
            InputSource = inputSource ?? throw new System.ArgumentNullException(nameof(inputSource));
            PlayerPhase = playerPhase ?? throw new System.ArgumentNullException(nameof(playerPhase));
            EnemyPhase = enemyPhase ?? throw new System.ArgumentNullException(nameof(enemyPhase));
            RecoveryStage = recoveryStage ?? throw new System.ArgumentNullException(nameof(recoveryStage));
            TimeAdvanceStage = timeAdvanceStage ?? throw new System.ArgumentNullException(nameof(timeAdvanceStage));
        }

        public ArenaRunSession Session { get; }
        public IGameClock Clock { get; }
        public IPlayerInputSource InputSource { get; }
        public PlayerTickPhase PlayerPhase { get; }
        public EnemyTickPhase EnemyPhase { get; }
        public PendingInteractionRecoveryStage RecoveryStage { get; }
        public TimeAdvanceStage TimeAdvanceStage { get; }
    }
}
