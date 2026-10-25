using System;
using Game.Arena.Application.Ports;
using Game.Arena.Infrastructure.Movement;
using UnityEngine;

namespace Game.CompositionRoot
{
    /// <summary>
    /// The Unity objects the Arena scene scope needs, supplied explicitly by the
    /// scene binder. Nothing is discovered through a global object search.
    /// </summary>
    public sealed class ArenaSceneObjects
    {
        public ArenaSceneObjects(
            ArenaGameConfiguration configuration,
            Camera camera,
            Transform player,
            IPlayerInputSource inputSource,
            IEnemyTransformRegistry enemyTransforms)
        {
            Configuration = configuration
                ?? throw new ArgumentNullException(nameof(configuration));

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
        }

        public ArenaGameConfiguration Configuration { get; }

        public Camera Camera { get; }

        public Transform Player { get; }

        public IPlayerInputSource InputSource { get; }

        public IEnemyTransformRegistry EnemyTransforms { get; }
    }
}
