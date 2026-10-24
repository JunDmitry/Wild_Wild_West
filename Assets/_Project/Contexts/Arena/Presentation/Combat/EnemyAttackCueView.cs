using System;
using Game.Arena.Domain.Geometry;
using UnityEngine;

namespace Game.Arena.Presentation.Combat
{
    public sealed class EnemyAttackCueView : MonoBehaviour
    {
        public const string StatusLabel = "Enemy attack";

        [SerializeField] private LineRenderer _line;
        [SerializeField] private float _height = 2.5f;
        [SerializeField] private float _size = 0.35f;

        public string Label => StatusLabel;

        public bool IsVisible
        {
            get
            {
                LineRenderer line = Line;
                return line != null && line.enabled;
            }
        }

        public Position3D Position { get; private set; }
        private LineRenderer Line => _line != null ? _line : GetComponent<LineRenderer>();

        public void Show(Position3D enemyPosition)
        {
            LineRenderer line = RequireLine();
            Vector3 origin = UnityGeometry.ToVector3(enemyPosition);

            Position = enemyPosition;

            line.positionCount = 3;
            line.SetPosition(0, origin + new Vector3(-_size, _height + _size, 0f));
            line.SetPosition(1, origin + new Vector3(0f, _height, 0f));
            line.SetPosition(2, origin + new Vector3(_size, _height + _size, 0f));
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
                throw new InvalidOperationException("EnemyAttackCueView requires a LineRenderer.");
            }

            return line;
        }
    }
}
