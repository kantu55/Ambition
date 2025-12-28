using Ambition.DataStructures;
using System;
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
                string costInfo = "コスト:\n";
                if (currentAction.CostMoney != 0)
                {
                    costInfo += $"  資金: {currentAction.CostMoney:N0}円\n";
                }
                if (currentAction.CostWifeHealth != 0)
                {
                    costInfo += $"  妻の体力: {currentAction.CostWifeHealth}\n";
                }
                actionCostText.text = costInfo;
            }

            // 効果情報
            if (actionEffectsText != null)
            {
                string effectsInfo = "効果:\n";
                
                if (currentAction.HealthChange != 0)
                {
                    effectsInfo += $"  夫の体力: {currentAction.HealthChange:+#;-#;0}\n";
                }
                if (currentAction.MentalChange != 0)
                {
                    effectsInfo += $"  夫のメンタル: {currentAction.MentalChange:+#;-#;0}\n";
                }
                if (currentAction.FatigueChange != 0)
                {
                    effectsInfo += $"  夫の疲労: {currentAction.FatigueChange:+#;-#;0}\n";
                }
                if (currentAction.LoveChange != 0)
                {
                    effectsInfo += $"  夫の愛情: {currentAction.LoveChange:+#;-#;0}\n";
                }
                if (currentAction.MuscleChange != 0)
                {
                    effectsInfo += $"  筋力: {currentAction.MuscleChange:+#;-#;0}\n";
                }
                if (currentAction.TechniqueChange != 0)
                {
                    effectsInfo += $"  技術: {currentAction.TechniqueChange:+#;-#;0}\n";
                }
                if (currentAction.ConcentrationChange != 0)
                {
                    effectsInfo += $"  集中: {currentAction.ConcentrationChange:+#;-#;0}\n";
                }
                
                actionEffectsText.text = effectsInfo;
            }
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
