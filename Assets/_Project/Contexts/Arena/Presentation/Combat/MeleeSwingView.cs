using System;
using Game.Arena.Domain.Geometry;
using UnityEngine;

namespace Game.Arena.Presentation.Combat
{
    public sealed class MeleeSwingView : MonoBehaviour
    {
        private const int ArcSegments = 24;

        [SerializeField] private LineRenderer _line;
        [SerializeField] private float _reach = 2f;
        [SerializeField] private float _arcDegrees = 360f;

        private Position3D _origin = Position3D.Zero;
        private Direction3D _direction = Direction3D.Forward;

        public float Reach
        {
            get => _reach;
            set => _reach = value;
        }

        public float ArcDegrees
        {
            get => _arcDegrees;
            set => _arcDegrees = value;
        }

        public Direction3D Direction => _direction;

        private LineRenderer Line => _line != null ? _line : GetComponent<LineRenderer>();

        public bool IsVisible
        {
            get
            {
                LineRenderer line = Line;
                return line != null && line.enabled;
            }
        }

        public void Show(Position3D origin, Direction3D direction)
        {
            _origin = origin;
            _direction = direction;

            LineRenderer line = RequireLine();
            Vector3 forward = UnityGeometry.ToVector3(direction);
            Vector3 center = UnityGeometry.ToVector3(origin);
            float halfAngle = Mathf.Clamp(_arcDegrees, 0f, 360f) * 0.5f;

            line.positionCount = ArcSegments + 1;

            for (int index = 0; index <= ArcSegments; index++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, (float)index / ArcSegments);
                Vector3 offset = Quaternion.AngleAxis(angle, Vector3.up) * forward;
                line.SetPosition(index, center + (offset * _reach));
            }

            line.enabled = true;
        }

        public void Hide()
        {
            LineRenderer line = Line;

            if (line != null)
            {
                line.enabled = false;
            }
        }

        public bool Contains(Position3D point)
        {
            Displacement3D offset = new(
                point.X - _origin.X,
                0f,
                point.Z - _origin.Z);

            if (offset.Length > _reach)
            {
                return false;
            }

            if (_arcDegrees >= 360f || offset.IsZero)
            {
                return true;
            }

            float halfAngle = Mathf.Clamp(_arcDegrees, 0f, 360f) * 0.5f * Mathf.Deg2Rad;
            return _direction.Dot(Direction3D.From(offset)) >= Mathf.Cos(halfAngle);
        }

        private LineRenderer RequireLine()
        {
            LineRenderer line = Line;

            if (line == null)
            {
                throw new InvalidOperationException("MeleeSwingView requires a LineRenderer.");
            }

            return line;
        }
    }
}
