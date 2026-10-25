using Game.Arena.Domain.Geometry;
using Game.Arena.Presentation.Combat;
using UnityEngine;

namespace Game.Arena.Presentation.Enemies
{
    public sealed class EnemyView : MonoBehaviour
    {
        [SerializeField] private Transform _body;
        [SerializeField] private Collider _targetCollider;

        public Transform Body => _body != null ? _body : transform;

        public Collider TargetCollider
        {
            get => _targetCollider != null ? _targetCollider : GetComponent<Collider>();
        }

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
