using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Architecture.UI.Views.HealthFeature
{
    public class SliderHealthIndicator : HealthIndicator
    {
        [SerializeField] private Slider _sliderUI;

        public override void UpdateView(HealthViewData data)
        {
            ChangeBarValue(data.Current, data.Max);
        }

        protected override void OnHide()
        {
            _sliderUI.gameObject.SetActive(false);
        }

        protected override void OnShow()
        {
            _sliderUI.gameObject.SetActive(true);
        }

        private void ChangeBarValue(float current, float max)
        {
            float percentile = current / max;
            float range = _sliderUI.maxValue - _sliderUI.minValue;

            _sliderUI.value = _sliderUI.minValue + percentile * range;
        }
    }
}