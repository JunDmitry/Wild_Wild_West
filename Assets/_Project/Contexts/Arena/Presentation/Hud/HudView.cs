using System;
using TMPro;
using UnityEngine;

namespace Game.Arena.Presentation.Hud
{
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _equippedWeaponText;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Transform _healthFill;
        [SerializeField] private TextMeshProUGUI _waveText;

        [SerializeField] private TextMeshProUGUI _bossLabel;
        [SerializeField] private TextMeshProUGUI _bossHealthText;
        [SerializeField] private Transform _bossFill;
        [SerializeField] private Transform _bossViewRoot;

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

        public string BossLabel { get; private set; }

        public int BossHealthCurrent { get; private set; }

        public int BossHealthMaximum { get; private set; }

        public string BossHealthText { get; private set; }

        public float BossHealthFraction { get; private set; }

        public bool IsBossPresent { get; private set; }

        public bool IsShown => EquippedWeaponText != null;

        public TextMeshProUGUI HealthTextMesh
        {
            get => _healthText;
            set => _healthText = value;
        }

        public Transform HealthFill
        {
            get => _healthFill;
            set => _healthFill = value;
        }

        public TextMeshProUGUI WaveTextMesh
        {
            get => _waveText;
            set => _waveText = value;
        }

        public TextMeshProUGUI BossLabelMesh
        {
            get => _bossLabel;
            set => _bossLabel = value;
        }

        public TextMeshProUGUI BossHealthTextMesh
        {
            get => _bossHealthText;
            set => _bossHealthText = value;
        }

        public Transform BossFill
        {
            get => _bossFill;
            set => _bossFill = value;
        }

        private TextMeshProUGUI EquippedWeaponTextMesh => _equippedWeaponText != null ? _equippedWeaponText : GetComponent<TextMeshProUGUI>();

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

            TextMeshProUGUI textMesh = EquippedWeaponTextMesh;

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

            if (fraction is < 0f or > 1f)
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

        public void ShowBoss(
            string label,
            string text,
            int current,
            int maximum,
            float fraction)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentException("Boss label cannot be empty.", nameof(label));
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Boss text cannot be empty.", nameof(text));
            }

            if (maximum <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximum));
            }

            if (current < 0 || current > maximum)
            {
                throw new ArgumentOutOfRangeException(nameof(current));
            }

            if (fraction is < 0f or > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(fraction));
            }

            BossLabel = label;
            BossHealthCurrent = current;
            BossHealthMaximum = maximum;
            BossHealthText = text;
            BossHealthFraction = fraction;
            IsBossPresent = true;

            if (_bossLabel != null)
            {
                _bossLabel.text = label;
            }

            if (_bossHealthText != null)
            {
                _bossHealthText.text = text;
            }

            if (_bossFill != null)
            {
                Vector3 scale = _bossFill.localScale;
                _bossFill.localScale = new Vector3(fraction, scale.y, scale.z);
            }

            if (_bossViewRoot != null)
            {
                _bossViewRoot.gameObject.SetActive(true);
            }
        }

        public void HideBoss()
        {
            BossHealthCurrent = 0;
            BossHealthMaximum = 0;
            BossHealthText = string.Empty;
            BossHealthFraction = 0f;
            IsBossPresent = false;

            if (_bossLabel != null)
            {
                _bossLabel.text = string.Empty;
            }

            if (_bossHealthText != null)
            {
                _bossHealthText.text = string.Empty;
            }

            if (_bossFill != null)
            {
                Vector3 scale = _bossFill.localScale;
                _bossFill.localScale = new Vector3(0f, scale.y, scale.z);
            }

            if (_bossViewRoot != null)
            {
                _bossViewRoot.gameObject.SetActive(false);
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
    }
}
