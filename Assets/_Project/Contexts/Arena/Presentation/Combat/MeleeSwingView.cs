using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using UnityEngine;

namespace Game.Arena.Presentation.Combat
{
    public sealed class MeleeSwingView : MonoBehaviour
    {
        private const int ArcSegments = 24;

        [SerializeField] private LineRenderer _line;

        private Position3D _origin = Position3D.Zero;
        private Direction3D _direction = Direction3D.Forward;
        private Distance _range;
        private AttackArc _arc;

        public Direction3D Direction => _direction;

        public Distance Range => _range;

        public AttackArc Arc => _arc;

        private LineRenderer Line => _line != null ? _line : GetComponent<LineRenderer>();

        public bool IsVisible
        {
            get
            {
                LineRenderer line = Line;
                return line != null && line.enabled;
            }
        }

        public void Show(Position3D origin, Direction3D direction, Distance range, AttackArc arc)
        {
            _origin = origin;
            _direction = direction;
            _range = range;
            _arc = arc;

            LineRenderer line = RequireLine();
            Vector3 forward = UnityGeometry.ToVector3(direction);
            Vector3 center = UnityGeometry.ToVector3(origin);
            float halfAngle = arc.Degrees * 0.5f;

            line.positionCount = ArcSegments + 1;

            for (int index = 0; index <= ArcSegments; index++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, (float)index / ArcSegments);
                Vector3 offset = Quaternion.AngleAxis(angle, Vector3.up) * forward;
                line.SetPosition(index, center + (offset * range.Value));
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

            if (offset.Length > _range.Value)
            {
                return false;
            }

            if (_arc.IsFull || offset.IsZero)
            {
                return true;
            }

            return _direction.Dot(Direction3D.From(offset)) >= _arc.CosHalfAngle;
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
