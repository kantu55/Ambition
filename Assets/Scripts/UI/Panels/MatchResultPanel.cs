using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI.Panels
{
    /// <summary>
    /// Panel for displaying match results (game performance, stats changes)
    /// </summary>
    public class MatchResultPanel : BaseGamePanel
    {
        [Header("Match Result UI")]
        [SerializeField] private TextMeshProUGUI matchTitleText;
        [SerializeField] private TextMeshProUGUI resultDescriptionText;
        [SerializeField] private TextMeshProUGUI statsChangeText;
        [SerializeField] private Button continueButton;

        /// <summary>
        /// Event fired when the user clicks continue
        /// </summary>
        public event Action OnContinueClicked;

        protected override void Awake()
        {
            base.Awake();

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(HandleContinueClicked);
            }
        }

        private void OnDestroy()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(HandleContinueClicked);
            }
        }

        /// <summary>
        /// Display match result data
        /// </summary>
        /// <param name="title">Match title (e.g., "vs 横浜DeNAベイスターズ")</param>
        /// <param name="description">Result description (e.g., "勝利 3-2")</param>
        /// <param name="statsChange">Stats changes text (e.g., "評価 +5")</param>
        public void ShowMatchResult(string title, string description, string statsChange = "")
        {
            Show();

            if (matchTitleText != null)
            {
                matchTitleText.text = title;
            }

            if (resultDescriptionText != null)
            {
                resultDescriptionText.text = description;
            }

            if (statsChangeText != null)
            {
                statsChangeText.text = statsChange;
            }
        }

        private void HandleContinueClicked()
        {
            OnContinueClicked?.Invoke();
            Hide();
        }
    }
}
