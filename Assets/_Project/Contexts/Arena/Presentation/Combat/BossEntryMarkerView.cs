using System;
using Game.Arena.Domain.Geometry;
using UnityEngine;

namespace Game.Arena.Presentation.Combat
{
    public sealed class BossEntryMarkerView : MonoBehaviour
    {
        public const string StatusLabel = "Boss";

        [SerializeField] private LineRenderer _line;
        [SerializeField] private float _height = 3.5f;
        [SerializeField] private float _size = 0.6f;

        public string Label => StatusLabel;

        public bool IsVisible
        {
            get
            {
                LineRenderer line = Line;
                return line != null && line.enabled && gameObject.activeSelf;
            }
        }

        public Position3D Position { get; private set; }

        private LineRenderer Line => _line != null ? _line : GetComponent<LineRenderer>();

        public void Show(Position3D bossPosition)
        {
            LineRenderer line = RequireLine();
            Vector3 origin = UnityGeometry.ToVector3(bossPosition);

            Position = bossPosition;

            line.positionCount = 5;
            line.SetPosition(0, origin + new Vector3(0f, _height + _size, 0f));
            line.SetPosition(1, origin + new Vector3(_size, _height, 0f));
            line.SetPosition(2, origin + new Vector3(0f, _height - _size, 0f));
            line.SetPosition(3, origin + new Vector3(-_size, _height, 0f));
            line.SetPosition(4, origin + new Vector3(0f, _height + _size, 0f));
            line.loop = true;
            line.enabled = true;

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            LineRenderer line = Line;

            if (line != null)
            {
                line.enabled = false;
            }

            gameObject.SetActive(false);
        }

        private LineRenderer RequireLine()
        {
            LineRenderer line = Line;

            if (line == null)
            {
                throw new InvalidOperationException("BossEntryMarkerView requires a LineRenderer.");
            }

            return line;
        }
    }
}
