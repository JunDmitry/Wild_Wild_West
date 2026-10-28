using System;
using TMPro;
using UnityEngine;

namespace Game.Arena.Presentation.Accessibility
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class Notification : MonoBehaviour
    {
        // This is test class
        [SerializeField] private TextMeshProUGUI _text;

        private CanvasGroup _canvasGroup;
        private float _originalAlpha;
        private Vector3 _originalScale;

        public float OriginalAlpha => _originalAlpha;
        public Vector3 OriginalScale => _originalScale;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _originalAlpha = _canvasGroup.alpha;
            _originalScale = transform.localScale;
        }

        public void Initialize(string text)
        {
            if (_text != null && (string.IsNullOrEmpty(text) == false) && (string.Equals(_text.text, text, StringComparison.Ordinal) == false))
            {
                _text.text = text;
            }
        }

        public void Show()
        {
            if (_text != null)
            {
                _canvasGroup.gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            if (_text != null)
            {
                _canvasGroup.gameObject.SetActive(false);
            }
        }

        public void SetAlpha(float alpha)
        {
            if (alpha is < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException($"Alpha should between 0 and 1, but was {alpha}.");
            }

            _canvasGroup.alpha = alpha;
        }

        public void Reset()
        {
            _canvasGroup.alpha = _originalAlpha;
            transform.localScale = _originalScale;
        }
    }
}
