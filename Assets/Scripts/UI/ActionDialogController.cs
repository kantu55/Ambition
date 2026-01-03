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

            if (currentAction.CashCost != 0)
            {
                stringBuilder.Append($"資金: {currentAction.CashCost:N0}円\n");
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
            if (currentAction.DeltaHP != 0)
            {
                stringBuilder.Append($"夫体力: {FormatChangeValue(currentAction.DeltaHP)}\n");
            }

            if (currentAction.DeltaMP != 0)
            {
                stringBuilder.Append($"夫精神: {FormatChangeValue(currentAction.DeltaMP)}\n");
            }

            if (currentAction.DeltaCOND != 0)
            {
                stringBuilder.Append($"夫調子: {FormatChangeValue(currentAction.DeltaCOND)}\n");
            }

            if (currentAction.DeltaLove != 0)
            {
                stringBuilder.Append($"愛情: {FormatChangeValue(currentAction.DeltaLove)}\n");
            }

            if (currentAction.DeltaPublicEye != 0)
            {
                stringBuilder.Append($"世間の目: {FormatChangeValue(currentAction.DeltaPublicEye)}\n");
            }

            if (currentAction.DeltaTeamEvaluation != 0)
            {
                stringBuilder.Append($"チーム評価: {FormatChangeValue(currentAction.DeltaTeamEvaluation)}\n");
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
