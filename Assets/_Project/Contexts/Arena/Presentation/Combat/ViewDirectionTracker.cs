using Game.Arena.Domain.Geometry;
using UnityEngine;

namespace Game.Arena.Presentation.Combat
{
    public sealed class ViewDirectionTracker : MonoBehaviour
    {
        [SerializeField] private Transform _viewTransform;

        private Direction3D _lastDirection = Direction3D.Forward;

        public Transform ViewTransform
        {
            get => _viewTransform;
            set => _viewTransform = value;
        }

        public Direction3D Current
        {
            get
            {
                Transform view = _viewTransform != null
                    ? _viewTransform
                    : UnityEngine.Camera.main != null ? UnityEngine.Camera.main.transform : null;

                if (view != null)
                {
                    Vector3 forward = view.forward;

                    if (Direction3D.TryFrom(
                            new Displacement3D(forward.x, 0f, forward.z),
                            out Direction3D direction))
                    {
                        _lastDirection = direction;
                    }
                }

                return _lastDirection;
            }
        }
    }
}
