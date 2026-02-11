using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI.Panels
{
    /// <summary>
    /// マッチ結果パネルのクラス
    /// </summary>
    public class MatchResultPanel : BaseGamePanel
    {
        [SerializeField] private TextMeshProUGUI matchTitleText;
        [SerializeField] private TextMeshProUGUI resultDescriptionText;
        [SerializeField] private TextMeshProUGUI statsChangeText;
        [SerializeField] private Button continueButton;

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