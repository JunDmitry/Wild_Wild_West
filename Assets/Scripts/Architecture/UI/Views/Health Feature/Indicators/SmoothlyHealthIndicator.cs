using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Architecture.UI.Views.HealthFeature
{
    public sealed class SmoothlyHealthIndicator : SliderHealthIndicator
    {
        [SerializeField, Min(0)] private float _smoothDuration = 0.25f;

        private Coroutine _coroutine;
        private float _previousCurrentHealth;

        public override void UpdateView(HealthViewData data)
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);

            _coroutine = StartCoroutine(SmoothlyChangeValue(_previousCurrentHealth, data.Current, data.Max));
            _previousCurrentHealth = data.Current;
        }

        private IEnumerator SmoothlyChangeValue(float source, float target, float maxHealth)
        {
            float healthRange = target - source;
            float elapsedSeconds = 0f;
            float nextValue;

            while (elapsedSeconds < _smoothDuration)
            {
                elapsedSeconds += Time.deltaTime;
                nextValue = Mathf.Lerp(source, target, elapsedSeconds / _smoothDuration);
                base.UpdateView(new(source + healthRange * nextValue, maxHealth));

                yield return null;
            }

            _coroutine = null;
        }
    }
}