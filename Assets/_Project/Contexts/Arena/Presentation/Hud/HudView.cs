using System;
using UnityEngine;

namespace Game.Arena.Presentation.Hud
{
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private TextMesh _equippedWeaponText;
        [SerializeField] private TextMesh _healthText;
        [SerializeField] private Transform _healthFill;
        [SerializeField] private TextMesh _waveText;

        public string EquippedWeaponLabel { get; private set; }

        public string EquippedWeaponText { get; private set; }

        public string HealthLabel { get; private set; }

        public int HealthCurrent { get; private set; }

        public int HealthMaximum { get; private set; }

        public string HealthText { get; private set; }

        public float HealthFraction { get; private set; }

        public bool IsDefeated { get; private set; }

        public string WaveLabel { get; private set; }

        public string WaveText { get; private set; }

        public bool IsShown => EquippedWeaponText != null;

        public TextMesh HealthTextMesh
        {
            get => _healthText;
            set => _healthText = value;
        }

        public Transform HealthFill
        {
            get => _healthFill;
            set => _healthFill = value;
        }

        public TextMesh WaveTextMesh
        {
            get => _waveText;
            set => _waveText = value;
        }

        public void ShowEquippedWeapon(string label, string text)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentException("Equipped weapon label cannot be empty.", nameof(label));
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Equipped weapon text cannot be empty.", nameof(text));
            }

            EquippedWeaponLabel = label;
            EquippedWeaponText = text;

            TextMesh textMesh = EquippedWeaponTextMesh;

            if (textMesh != null)
            {
                textMesh.text = label + ": " + text;
            }
        }

        public void ShowHealth(
            string label,
            string text,
            int current,
            int maximum,
            float fraction,
            bool isDefeated)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentException("Health label cannot be empty.", nameof(label));
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Health text cannot be empty.", nameof(text));
            }

            if (maximum <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximum));
            }

            if (current < 0 || current > maximum)
            {
                throw new ArgumentOutOfRangeException(nameof(current));
            }

            if (fraction < 0f || fraction > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(fraction));
            }

            HealthLabel = label;
            HealthCurrent = current;
            HealthMaximum = maximum;
            HealthText = text;
            HealthFraction = fraction;
            IsDefeated = isDefeated;

            if (_healthText != null)
            {
                _healthText.text = label + ": " + text;
            }

            if (_healthFill != null)
            {
                Vector3 scale = _healthFill.localScale;
                _healthFill.localScale = new Vector3(fraction, scale.y, scale.z);
            }
        }

        public void ShowWave(string label, string text)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentException("Wave label cannot be empty.", nameof(label));
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Wave text cannot be empty.", nameof(text));
            }

            WaveLabel = label;
            WaveText = text;

            if (_waveText != null)
            {
                _waveText.text = text;
            }
        }

        private TextMesh EquippedWeaponTextMesh =>
            _equippedWeaponText != null ? _equippedWeaponText : GetComponent<TextMesh>();
    }
}
