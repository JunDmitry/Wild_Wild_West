using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Sessions
{
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

        public DefeatedRunRestartOutcome RestartDefeatedRun()
        {
            if (HasActiveRun == false)
            {
                return DefeatedRunRestartOutcome.NoActiveRun;
            }

            ArenaRun currentRun = GetRequiredActiveRun();

            if (currentRun.Status != ArenaRunStatus.Defeat)
            {
                return DefeatedRunRestartOutcome.ActiveRunIsNotDefeated;
            }

            ArenaRunId newArenaRunId = _dependencies.ArenaRunIdSource.Allocate();
            PlayerId newPlayerId = _dependencies.PlayerIdSource.Allocate();

            ArenaRun newArenaRun = _dependencies.ArenaRunFactory.Start(
                newArenaRunId,
                newPlayerId,
                _creationParameters.ArenaDefinition,
                _creationParameters.PlayerDefinition,
                _creationParameters.EnemyCatalog,
                _creationParameters.WeaponCatalog,
                _creationParameters.WaveCatalog);

            _dependencies.Repository.Add(newArenaRun);

            bool removed = _dependencies.Repository.Remove(currentRun.Id);

            if (removed == false)
            {
                _dependencies.Repository.Remove(newArenaRunId);

                throw new InvalidOperationException("Failed to remove the previous defeated ArenaRun from the repository.");
            }

            _activeArenaRunId = newArenaRunId;

            return DefeatedRunRestartOutcome.Restarted(newArenaRunId, newPlayerId);
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
