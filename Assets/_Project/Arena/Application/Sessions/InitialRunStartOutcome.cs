using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Sessions
{
    public sealed class InitialRunStartOutcome
    {
        private InitialRunStartOutcome(
            InitialRunStartStatus status,
            ArenaRunId arenaRunId,
            PlayerId playerId)
        {
            Status = status;
            ArenaRunId = arenaRunId;
            PlayerId = playerId;
        }

        public InitialRunStartStatus Status { get; }

        public ArenaRunId ArenaRunId { get; }

        public PlayerId PlayerId { get; }

        public bool IsStarted => Status == InitialRunStartStatus.Started;

        public static InitialRunStartOutcome Started(
            ArenaRunId arenaRunId,
            PlayerId playerId)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (playerId.IsNone)
            {
                throw new ArgumentException("PlayerId cannot be None.", nameof(playerId));
            }

            return new InitialRunStartOutcome(
                InitialRunStartStatus.Started,
                arenaRunId,
                playerId);
        }

        public static InitialRunStartOutcome ActiveRunAlreadyExists()
        {
            return new InitialRunStartOutcome(
                InitialRunStartStatus.ActiveRunAlreadyExists,
                ArenaRunId.None,
                PlayerId.None);
        }
    }

    public sealed class ArenaRunSession
    {
        private readonly ArenaRunSessionDependencies _dependencies;
        private readonly ArenaRunCreationParameters _creationParameters;

        private ArenaRunId _activeArenaRunId;

        public ArenaRunSession(
            ArenaRunSessionDependencies dependencies,
            ArenaRunCreationParameters creationParameters)
        {
            if (dependencies == null)
            {
                throw new ArgumentNullException(nameof(dependencies));
            }

            if (creationParameters == null)
            {
                throw new ArgumentNullException(nameof(creationParameters));
            }

            _dependencies = dependencies;
            _creationParameters = creationParameters;
            _activeArenaRunId = ArenaRunId.None;
        }

        public bool HasActiveRun => _activeArenaRunId.IsNone == false;

        public ArenaRunId ActiveArenaRunId => _activeArenaRunId;

        public InitialRunStartOutcome StartInitialRun()
        {
            if (HasActiveRun)
            {
                return InitialRunStartOutcome.ActiveRunAlreadyExists();
            }

            ArenaRunId arenaRunId = _dependencies.ArenaRunIdSource.Allocate();
            PlayerId playerId = _dependencies.PlayerIdSource.Allocate();

            ArenaRun arenaRun = _dependencies.ArenaRunFactory.Start(
                arenaRunId,
                playerId,
                _creationParameters.ArenaDefinition,
                _creationParameters.PlayerDefinition,
                _creationParameters.EnemyCatalog,
                _creationParameters.WeaponCatalog,
                _creationParameters.WaveCatalog);

            _dependencies.Repository.Add(arenaRun);
            _activeArenaRunId = arenaRunId;

            return InitialRunStartOutcome.Started(arenaRunId, playerId);
        }

        internal ArenaRun GetRequiredActiveRun()
        {
            if (HasActiveRun == false)
            {
                throw new InvalidOperationException("No active ArenaRun exists.");
            }

            bool found = _dependencies.Repository.TryGet(_activeArenaRunId, out ArenaRun arenaRun);

            if (found == false)
            {
                throw new InvalidOperationException("ActiveArenaRunId does not correspond to any aggregate in the repository.");
            }

            return arenaRun;
        }
    }
}
