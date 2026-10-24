using System;
using Game.Arena.Domain.Identity;
using UnityEngine;

namespace Game.Arena.Presentation.Windows
{
    public sealed class DefeatView : MonoBehaviour
    {
        public const string Statement = "Defeat";
        public const string WaveReachedLabel = "Wave reached";

        [SerializeField] private TextMesh _statementText;
        [SerializeField] private TextMesh _waveText;

        public TextMesh StatementTextMesh
        {
            get => _statementText;
            set => _statementText = value;
        }

        public TextMesh WaveTextMesh
        {
            get => _waveText;
            set => _waveText = value;
        }

        public bool IsShown { get; private set; }

        public ArenaRunId ShownRunId { get; private set; } = ArenaRunId.None;

        public void Show(ArenaRunId arenaRunId, string waveText)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (string.IsNullOrWhiteSpace(waveText))
            {
                throw new ArgumentException("Wave text cannot be empty.", nameof(waveText));
            }

            if (IsShown && ShownRunId == arenaRunId)
            {
                return;
            }

            ShownRunId = arenaRunId;
            IsShown = true;

            if (_statementText != null)
            {
                _statementText.text = Statement;
            }

            if (_waveText != null)
            {
                _waveText.text = waveText;
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            IsShown = false;
            ShownRunId = ArenaRunId.None;

            gameObject.SetActive(false);
        }
    }
}
