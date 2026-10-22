using UnityEngine;

namespace Game.Arena.Presentation.Player
{
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private Transform body;

        public Transform Body => body != null ? body : transform;

        public void SetWorldPosition(Vector3 position)
        {
            Body.position = position;
        }
    }
}
