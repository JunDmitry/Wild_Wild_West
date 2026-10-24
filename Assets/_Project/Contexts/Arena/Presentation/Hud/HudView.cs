using System;
using UnityEngine;

namespace Game.Arena.Presentation.Hud
{
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private TextMesh _equippedWeaponText;

        public string EquippedWeaponLabel { get; private set; }

        public string EquippedWeaponText { get; private set; }

        public bool IsShown => EquippedWeaponText != null;

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

        private TextMesh EquippedWeaponTextMesh =>
            _equippedWeaponText != null ? _equippedWeaponText : GetComponent<TextMesh>();
    }
}
