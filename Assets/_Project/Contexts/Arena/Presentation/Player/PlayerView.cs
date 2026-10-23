using UnityEngine;

namespace Game.Arena.Presentation.Player
{
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private Transform _body;

        public Transform Body => _body != null ? _body : transform;

        public void SetWorldPosition(Vector3 position)
        {
            Body.position = position;
        }
    }
}
