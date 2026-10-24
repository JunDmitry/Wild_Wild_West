using Game.Arena.Domain.Geometry;
using UnityEngine;

namespace Game.Arena.Presentation.Combat
{
    public sealed class HitConfirmationView : MonoBehaviour
    {
        [SerializeField] private float _lifetimeSeconds = 0.25f;

        private float _remainingSeconds;

        public float LifetimeSeconds
        {
            get => _lifetimeSeconds;
            set => _lifetimeSeconds = value;
        }

        public bool IsVisible { get; private set; }

        public Position3D Position { get; private set; }

        public float RemainingSeconds => _remainingSeconds;

        public void Show(Position3D position)
        {
            Position = position;
            transform.position = UnityGeometry.ToVector3(position);
            gameObject.SetActive(true);

            IsVisible = true;
            _remainingSeconds = _lifetimeSeconds;
        }

        public void Hide()
        {
            IsVisible = false;
            _remainingSeconds = 0f;
            gameObject.SetActive(false);
        }

        public bool Advance(float deltaSeconds)
        {
            if (IsVisible == false)
            {
                return false;
            }

            _remainingSeconds -= deltaSeconds;

            if (_remainingSeconds > 0f)
            {
                return false;
            }

            Hide();

            return true;
        }
    }
}
