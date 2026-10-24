using System;
using Game.Arena.Domain.Geometry;
using UnityEngine;

namespace Game.Arena.Presentation.Combat
{
    public sealed class FirearmAimMarkerView : MonoBehaviour
    {
        [SerializeField] private LineRenderer _line;
        [SerializeField] private Transform _endMarker;
        [SerializeField] private float _length = 10f;

        public float Length
        {
            get => _length;
            set => _length = value;
        }

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
            LineRenderer line = RequireLine();
            Vector3 start = UnityGeometry.ToVector3(origin);
            Vector3 end = start + (UnityGeometry.ToVector3(direction) * _length);

            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.enabled = true;

            if (_endMarker != null)
            {
                _endMarker.position = end;
                _endMarker.gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            LineRenderer line = Line;

            if (line != null)
            {
                line.enabled = false;
            }

            if (_endMarker != null)
            {
                _endMarker.gameObject.SetActive(false);
            }
        }

        private LineRenderer Line => _line != null ? _line : GetComponent<LineRenderer>();

        private LineRenderer RequireLine()
        {
            LineRenderer line = Line;

            if (line == null)
            {
                throw new InvalidOperationException("FirearmAimMarkerView requires a LineRenderer.");
            }

            line.positionCount = 2;
            return line;
        }
    }
}
