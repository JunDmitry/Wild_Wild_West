using System;
using System.Collections.Generic;
using Game.Arena.Application.Hud;
using Game.Arena.Application.Notifications;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Ticks
{
    /// <summary>
    /// The per-run two-slot frame: each slot owns a published snapshot, its
    /// HUD snapshot, its tick result and its notification pool. A tick fills
    /// the slot the previous tick did not publish, so a published result and
    /// everything reachable from it stays valid for one further tick
    /// (ADR-0035 point 2).
    /// </summary>
    internal sealed class ArenaTickFrameBuffers
    {
        private readonly ArenaRunSnapshotMapper _snapshotMapper = new();
        private readonly ArenaRunHudSnapshotMapper _hudMapper = new();
        private readonly ArenaRunTickRecorder _recorder = new();
        private readonly Slot[] _slots;
        private readonly ArenaRunStateSnapshot _stateSnapshot;

        private int _active;

        public ArenaTickFrameBuffers(int enemyCapacity, ArenaRunStateSnapshot stateSnapshot)
        {
            if (enemyCapacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(enemyCapacity));
            }

            _stateSnapshot = stateSnapshot ?? throw new ArgumentNullException(nameof(stateSnapshot));
            _slots = new Slot[2];
            _slots[0] = new Slot(enemyCapacity);
            _slots[1] = new Slot(enemyCapacity);

            // The first BeginTick switches to slot 0.
            _active = 1;
        }

        public ArenaRunTickRecorder Recorder => _recorder;

        public ArenaRunStateSnapshot StateSnapshot => _stateSnapshot;

        public void BeginTick()
        {
            _active ^= 1;
            _slots[_active].Release();
            _recorder.Reset();
        }

        public ArenaRunTickResult BuildResult(ArenaRun run, ArenaRunStateSnapshot stateSnapshot)
        {
            if (run == null)
            {
                throw new ArgumentNullException(nameof(run));
            }

            Slot slot = _slots[_active];

            _snapshotMapper.Map(stateSnapshot, slot.Snapshot);
            _hudMapper.Map(slot.Snapshot, slot.Hud);
            slot.NotificationPool.Fill(_recorder.Events, slot.NotificationList);

            return _recorder.BuildInto(
                slot.Result,
                run.Id,
                run.Revision,
                slot.Snapshot,
                slot.Hud,
                slot.NotificationList);
        }

        private sealed class Slot
        {
            public Slot(int enemyCapacity)
            {
                Snapshot = new ArenaRunSnapshot(enemyCapacity);
                Hud = new ArenaRunHudSnapshot();
                Result = new ArenaRunTickResult();
                NotificationList = new List<IArenaRunNotification>(enemyCapacity);
                NotificationPool = new ArenaRunNotificationPool(enemyCapacity);
            }

            public ArenaRunSnapshot Snapshot { get; }

            public ArenaRunHudSnapshot Hud { get; }

            public ArenaRunTickResult Result { get; }

            public List<IArenaRunNotification> NotificationList { get; }

            public ArenaRunNotificationPool NotificationPool { get; }

            public void Release()
            {
                NotificationPool.Reset();
                NotificationList.Clear();
            }
        }
    }
}
