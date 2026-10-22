using System;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Geometry;
using UnityEngine;

namespace Game.Arena.Presentation.Player
{
    public sealed class PlayerPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerView playerView;

        public void Present(ArenaRunSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            PlayerView view = playerView != null ? playerView : GetComponent<PlayerView>();

            if (view == null)
            {
                throw new InvalidOperationException("PlayerPresenter requires a PlayerView.");
            }

            Position3D position = snapshot.Player.Position;
            view.SetWorldPosition(new Vector3(position.X, position.Y, position.Z));
        }
    }
}
