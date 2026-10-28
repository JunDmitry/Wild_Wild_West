using System;
using Game.Arena.Domain.Identity;
using TMPro;
using UnityEngine;

namespace Game.Arena.Presentation.Windows
{
    public sealed class VictoryView : MonoBehaviour
    {
        public const string Title = "Victory";
        public const string Message = "the run was won";

        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;

        public TextMeshProUGUI TitleTextMesh
        {
            get => _titleText;
            set => _titleText = value;
        }

        public TextMeshProUGUI MessageTextMesh
        {
            get => _messageText;
            set => _messageText = value;
        }

        public bool IsShown { get; private set; }

        public bool HoldsFocus { get; private set; }

        public ArenaRunId ShownRunId { get; private set; } = ArenaRunId.None;

        public void Show(ArenaRunId arenaRunId)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (IsShown && ShownRunId == arenaRunId)
            {
                return;
            }

            ShownRunId = arenaRunId;
            IsShown = true;
            HoldsFocus = true;

            if (_titleText != null)
            {
                _titleText.text = Title;
            }

            if (_messageText != null)
            {
                _messageText.text = Message;
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            IsShown = false;
            HoldsFocus = false;
            ShownRunId = ArenaRunId.None;

            gameObject.SetActive(false);
        }
    }
}
