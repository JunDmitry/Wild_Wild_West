using Game.Arena.Domain.Geometry;
using Game.Arena.Presentation.Combat;
using UnityEngine;

namespace Game.Arena.Presentation.Enemies
{
    public sealed class EnemyView : MonoBehaviour
    {
        [SerializeField] private Transform _body;

        public Transform Body => _body != null ? _body : transform;

        public Position3D Position { get; private set; }

        public bool IsVisible => gameObject.activeSelf;

        public void Show(Position3D position)
        {
            Position = position;
            Body.position = UnityGeometry.ToVector3(position);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
