using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI.Panels
{
    /// <summary>
    /// Panel for displaying end-of-month summaries and reports
    /// </summary>
    public class MonthlyReportPanel : BaseGamePanel
    {
        [Header("Monthly Report UI")]
        [SerializeField] private TextMeshProUGUI reportTitleText;
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private TextMeshProUGUI incomeText;
        [SerializeField] private TextMeshProUGUI expensesText;
        [SerializeField] private TextMeshProUGUI savingsText;
        [SerializeField] private TextMeshProUGUI playerStatsText;
        [SerializeField] private TextMeshProUGUI wifeStatsText;
        [SerializeField] private TextMeshProUGUI summaryText;
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
        /// Show monthly report
        /// </summary>
        public void ShowReport(
            string date,
            int income,
            int expenses,
            int savings,
            string playerStats = "",
            string wifeStats = "",
            string summary = "")
        {
            Show();

            if (reportTitleText != null)
            {
                reportTitleText.text = "月次報告";
            }

            if (dateText != null)
            {
                dateText.text = date;
            }

            if (incomeText != null)
            {
                incomeText.text = $"収入: ¥{income:N0}";
            }

            if (expensesText != null)
            {
                expensesText.text = $"支出: ¥{expenses:N0}";
            }

            if (savingsText != null)
            {
                savingsText.text = $"貯金: ¥{savings:N0}";
            }

            if (playerStatsText != null)
            {
                playerStatsText.text = playerStats;
            }

            if (wifeStatsText != null)
            {
                wifeStatsText.text = wifeStats;
            }

            if (summaryText != null)
            {
                summaryText.text = summary;
            }
        }

        private void HandleContinueClicked()
        {
            OnContinueClicked?.Invoke();
            Hide();
        }
    }
}
