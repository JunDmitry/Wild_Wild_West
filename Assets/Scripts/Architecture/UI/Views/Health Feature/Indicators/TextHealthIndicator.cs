using TMPro;
using UnityEngine;

namespace Assets.Scripts.Architecture.UI.Views.HealthFeature
{
    public class TextHealthIndicator : HealthIndicator
    {
        [SerializeField] private TextMeshProUGUI _textUI;

        public override void UpdateView(HealthViewData data)
        {
            ChangeText(data.Current, data.Max);
        }

        protected override void OnHide()
        {
            _textUI.gameObject.SetActive(false);
        }

        protected override void OnShow()
        {
            _textUI.gameObject.SetActive(true);
        }

        private void ChangeText(float current, float max)
        {
            _textUI.text = $"{current:F0}/{max:F0}";
        }
    }
}