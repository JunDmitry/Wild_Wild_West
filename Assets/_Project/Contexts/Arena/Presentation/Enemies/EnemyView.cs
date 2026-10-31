using Game.Arena.Domain.Geometry;
using Game.Arena.Presentation.Combat;
using UnityEngine;

namespace Game.Arena.Presentation.Enemies
{
    public sealed class EnemyView : MonoBehaviour
    {
        private const float MinimumHalfExtent = 1e-4f;

        [SerializeField] private Transform _body;
        [SerializeField] private Collider _targetCollider;

        private float _baseHalfExtent;

        public Transform Body => _body != null ? _body : transform;

        public Collider TargetCollider
        {
            get => _targetCollider != null ? _targetCollider : GetComponent<Collider>();
        }

        public Position3D Position { get; private set; }

        public bool IsVisible => gameObject.activeSelf;

        private void Awake()
        {
            CacheBaseHalfExtent();
        }

        public void Show(Position3D position, CollisionRadius collisionRadius)
        {
            Position = position;
            ApplyCollisionRadius(collisionRadius.Value.Value);
            Body.position = UnityGeometry.ToVector3(position);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void ApplyCollisionRadius(float radius)
        {
            if (_baseHalfExtent <= MinimumHalfExtent)
            {
                CacheBaseHalfExtent();
            }

            if (_baseHalfExtent <= MinimumHalfExtent)
            {
                return;
            }

            float scale = radius / _baseHalfExtent;
            transform.localScale = new Vector3(scale, scale, scale);
        }

        /// <summary>
        /// Measures the target collider's horizontal half-extent at the view's
        /// base scale, so the same prefab serves every Enemy kind: the root is
        /// then scaled by <c>radius / baseHalfExtent</c> (ADR-0039).
        /// </summary>
        private void CacheBaseHalfExtent()
        {
            Collider collider = TargetCollider;

            if (collider == null)
            {
                _baseHalfExtent = 0f;

                return;
            }

            float scale = Mathf.Max(MinimumHalfExtent, Mathf.Abs(transform.lossyScale.x));
            _baseHalfExtent = collider.bounds.extents.x / scale;
        }
    }
}
