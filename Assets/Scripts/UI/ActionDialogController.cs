using Ambition.DataStructures;
using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI
{
    /// <summary>
    /// アクション詳細ダイアログを管理するコントローラー
    /// </summary>
    public class ActionDialogController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private TextMeshProUGUI actionNameText;
        [SerializeField] private TextMeshProUGUI actionDescriptionText;
        [SerializeField] private TextMeshProUGUI actionCostText;
        [SerializeField] private TextMeshProUGUI actionEffectsText;
        [SerializeField] private Button executeButton;
        [SerializeField] private Button backButton;

        private WifeActionModel currentAction;
        private Action<WifeActionModel> onExecute;
        private Action onBack;
        private StringBuilder stringBuilder = new StringBuilder(512);

        private void Awake()
        {
            if (executeButton != null)
            {
                executeButton.onClick.AddListener(OnExecuteClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }
        }

        /// <summary>
        /// ダイアログを開く
        /// </summary>
        /// <param name="action">表示するアクション</param>
        /// <param name="onExecuteCallback">実行ボタン押下時のコールバック</param>
        /// <param name="onBackCallback">戻るボタン押下時のコールバック</param>
        public void Open(WifeActionModel action, Action<WifeActionModel> onExecuteCallback, Action onBackCallback = null)
        {
            currentAction = action;
            onExecute = onExecuteCallback;
            onBack = onBackCallback;

            DisplayActionDetails();

            if (dialogPanel != null)
            {
                dialogPanel.SetActive(true);
            }
        }

        /// <summary>
        /// ダイアログを閉じる
        /// </summary>
        public void Close()
        {
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
            }

            currentAction = null;
        }

        /// <summary>
        /// アクション詳細を表示
        /// </summary>
        private void DisplayActionDetails()
        {
            if (currentAction == null)
            {
                return;
            }

            // アクション名
            if (actionNameText != null)
            {
                actionNameText.text = currentAction.Name;
            }

            // 説明
            if (actionDescriptionText != null)
            {
                actionDescriptionText.text = currentAction.Description;
            }

            // コスト情報
            if (actionCostText != null)
            {
                stringBuilder.Clear();
                stringBuilder.Append("コスト:\n");
                if (currentAction.CostMoney != 0)
                {
                    stringBuilder.Append("  資金: ").Append(currentAction.CostMoney.ToString("N0")).Append("円\n");
                }
                if (currentAction.CostWifeHealth != 0)
                {
                    stringBuilder.Append("  妻の体力: ").Append(currentAction.CostWifeHealth).Append('\n');
                }
                actionCostText.text = stringBuilder.ToString();
            }

            // 効果情報
            if (actionEffectsText != null)
            {
                stringBuilder.Clear();
                stringBuilder.Append("効果:\n");
                
                if (currentAction.HealthChange != 0)
                {
                    stringBuilder.Append("  夫の体力: ").Append(FormatChangeValue(currentAction.HealthChange)).Append('\n');
                }
                if (currentAction.MentalChange != 0)
                {
                    stringBuilder.Append("  夫のメンタル: ").Append(FormatChangeValue(currentAction.MentalChange)).Append('\n');
                }
                if (currentAction.FatigueChange != 0)
                {
                    stringBuilder.Append("  夫の疲労: ").Append(FormatChangeValue(currentAction.FatigueChange)).Append('\n');
                }
                if (currentAction.LoveChange != 0)
                {
                    stringBuilder.Append("  夫の愛情: ").Append(FormatChangeValue(currentAction.LoveChange)).Append('\n');
                }
                if (currentAction.MuscleChange != 0)
                {
                    stringBuilder.Append("  筋力: ").Append(FormatChangeValue(currentAction.MuscleChange)).Append('\n');
                }
                if (currentAction.TechniqueChange != 0)
                {
                    stringBuilder.Append("  技術: ").Append(FormatChangeValue(currentAction.TechniqueChange)).Append('\n');
                }
                if (currentAction.ConcentrationChange != 0)
                {
                    stringBuilder.Append("  集中: ").Append(FormatChangeValue(currentAction.ConcentrationChange)).Append('\n');
                }
                
                actionEffectsText.text = stringBuilder.ToString();
            }
        }

        /// <summary>
        /// 変化量を表示用にフォーマット
        /// </summary>
        private string FormatChangeValue(int value)
        {
            if (value > 0)
            {
                return "+" + value;
            }
            return value.ToString();
        }

        /// <summary>
        /// 実行ボタンがクリックされた時の処理
        /// </summary>
        private void OnExecuteClicked()
        {
            onExecute?.Invoke(currentAction);
            Close();
        }

        /// <summary>
        /// 戻るボタンがクリックされた時の処理
        /// </summary>
        private void OnBackClicked()
        {
            Close();
            onBack?.Invoke();
        }
    }
}
