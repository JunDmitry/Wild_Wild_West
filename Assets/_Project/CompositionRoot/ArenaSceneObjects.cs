using System;
using Game.Arena.Application.Ports;
using Game.Arena.Infrastructure.Movement;
using UnityEngine;

namespace Game.CompositionRoot
{
    /// <summary>
    /// The Unity objects the Arena scene scope needs, supplied explicitly by the
    /// scene binder. Nothing is discovered through a global object search.
    /// Configuration is not part of the seam: it is a process-lifetime service
    /// the child scope resolves from its parent (ADR-0031). The reloader is an
    /// optional scene-scope override: production passes null and the process
    /// registration stands, while a rig may supply one to observe reload requests.
    /// </summary>
    public sealed class ArenaSceneObjects
    {
        public ArenaSceneObjects(
            Camera camera,
            Transform player,
            IPlayerInputSource inputSource,
            IEnemyTransformRegistry enemyTransforms,
            IArenaSceneReloader reloader = null)
        {
            Camera = camera != null
                ? camera
                : throw new ArgumentNullException(nameof(camera));

            Player = player != null
                ? player
                : throw new ArgumentNullException(nameof(player));

            InputSource = inputSource
                ?? throw new ArgumentNullException(nameof(inputSource));

            EnemyTransforms = enemyTransforms
                ?? throw new ArgumentNullException(nameof(enemyTransforms));

            Reloader = reloader;
        }

        public Camera Camera { get; }

        public Transform Player { get; }

        public IPlayerInputSource InputSource { get; }

        public IEnemyTransformRegistry EnemyTransforms { get; }

        public IArenaSceneReloader Reloader { get; }
    }
}
