using Ambition.DataStructures;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI
{
    /// <summary>
    /// 行動の詳細を表示し、実行・戻るボタンを管理するダイアログコントローラー
    /// </summary>
    public class ActionDialogController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private TextMeshProUGUI actionNameText;
        [SerializeField] private TextMeshProUGUI actionDescriptionText;
        [SerializeField] private TextMeshProUGUI actionCostText;
        [SerializeField] private TextMeshProUGUI actionEffectsText;
        [SerializeField] private Button executeButton;
        [SerializeField] private Button backButton;

        /// <summary>
        /// 実行ボタンが押された時のコールバック
        /// </summary>
        public event Action<WifeActionModel> OnExecutePressed;

        /// <summary>
        /// 戻るボタンが押された時のコールバック
        /// </summary>
        public event Action OnBackPressed;

        private WifeActionModel currentAction;
        
        // GC Allocを避けるためのStringBuilder
        private System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(256);
        
        // ヘッダーテキスト
        private const string COST_HEADER = "【コスト】\n";
        private const string EFFECTS_HEADER = "【効果】\n";
        private const string NONE_TEXT = "なし";

        private void Awake()
        {
            if (executeButton != null)
            {
                executeButton.onClick.AddListener(HandleExecutePressed);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackPressed);
            }

            // 初期状態では非表示
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
            }
        }

        /// <summary>
        /// ダイアログを開き、行動の詳細を表示
        /// </summary>
        public void Open(WifeActionModel action)
        {
            if (action == null)
            {
                return;
            }

            currentAction = action;

            if (dialogPanel != null)
            {
                dialogPanel.SetActive(true);
            }

            UpdateDialogContent();
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
        /// ダイアログの内容を更新
        /// </summary>
        private void UpdateDialogContent()
        {
            if (currentAction == null)
            {
                return;
            }

            // 行動名
            if (actionNameText != null)
            {
                actionNameText.text = currentAction.Name;
            }

            // 説明
            if (actionDescriptionText != null)
            {
                actionDescriptionText.text = currentAction.Description;
            }

            // コスト
            if (actionCostText != null)
            {
                actionCostText.text = BuildCostText();
            }

            // 効果
            if (actionEffectsText != null)
            {
                actionEffectsText.text = BuildEffectsText();
            }
        }

        /// <summary>
        /// コスト情報のテキストを生成
        /// </summary>
        private string BuildCostText()
        {
            stringBuilder.Clear();
            stringBuilder.Append(COST_HEADER);

            if (currentAction.CostMoney != 0)
            {
                stringBuilder.Append($"資金: {currentAction.CostMoney:N0}円\n");
            }

            if (currentAction.CostWifeHealth != 0)
            {
                stringBuilder.Append($"妻体力: {currentAction.CostWifeHealth}\n");
            }

            if (stringBuilder.Length == COST_HEADER.Length)
            {
                stringBuilder.Append(NONE_TEXT);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 効果情報のテキストを生成
        /// </summary>
        private string BuildEffectsText()
        {
            stringBuilder.Clear();
            stringBuilder.Append(EFFECTS_HEADER);

            // 夫への効果
            if (currentAction.HealthChange != 0)
            {
                stringBuilder.Append($"夫体力: {FormatChangeValue(currentAction.HealthChange)}\n");
            }

            if (currentAction.MentalChange != 0)
            {
                stringBuilder.Append($"夫精神: {FormatChangeValue(currentAction.MentalChange)}\n");
            }

            if (currentAction.FatigueChange != 0)
            {
                stringBuilder.Append($"夫疲労: {FormatChangeValue(currentAction.FatigueChange)}\n");
            }

            if (currentAction.LoveChange != 0)
            {
                stringBuilder.Append($"愛情: {FormatChangeValue(currentAction.LoveChange)}\n");
            }

            if (currentAction.MuscleChange != 0)
            {
                stringBuilder.Append($"筋力: {FormatChangeValue(currentAction.MuscleChange)}\n");
            }

            if (currentAction.TechniqueChange != 0)
            {
                stringBuilder.Append($"技術: {FormatChangeValue(currentAction.TechniqueChange)}\n");
            }

            if (currentAction.ConcentrationChange != 0)
            {
                stringBuilder.Append($"集中: {FormatChangeValue(currentAction.ConcentrationChange)}\n");
            }

            // 妻への効果
            if (currentAction.StressChange != 0)
            {
                stringBuilder.Append($"妻ストレス: {FormatChangeValue(currentAction.StressChange)}\n");
            }

            if (currentAction.SkillExp != 0)
            {
                stringBuilder.Append($"スキル経験値: {FormatChangeValue(currentAction.SkillExp)}\n");
            }

            if (stringBuilder.Length == EFFECTS_HEADER.Length)
            {
                stringBuilder.Append(NONE_TEXT);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 数値変化を表示用の文字列に変換（正の値には+を付ける）
        /// </summary>
        private string FormatChangeValue(int value)
        {
            if (value > 0)
            {
                return $"+{value}";
            }

            return value.ToString();
        }

        /// <summary>
        /// 実行ボタンが押された時の処理
        /// </summary>
        private void HandleExecutePressed()
        {
            if (currentAction != null)
            {
                OnExecutePressed?.Invoke(currentAction);
            }
        }

        /// <summary>
        /// 戻るボタンが押された時の処理
        /// </summary>
        private void HandleBackPressed()
        {
            OnBackPressed?.Invoke();
        }

        private void OnDestroy()
        {
            if (executeButton != null)
            {
                executeButton.onClick.RemoveListener(HandleExecutePressed);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HandleBackPressed);
            }
        }
    }
}
