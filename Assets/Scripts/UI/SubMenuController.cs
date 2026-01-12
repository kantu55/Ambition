using Ambition.DataStructures;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI
{
    /// <summary>
    /// サブメニュー（行動リスト）を表示・管理するコントローラー
    /// </summary>
    public class SubMenuController : MonoBehaviour
    {
        [SerializeField] private MainGameView mainGameView;

        [Header("UI Components")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Transform contentContainer;
        [SerializeField] private Button itemButtonPrefab;
        [SerializeField] private Button backButton;

        [Header("Detail Panel Components")]
        [SerializeField] private GameObject detailPanelRoot;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailCostText;
        [SerializeField] private TextMeshProUGUI detailEffectText;
        [SerializeField] private Image detailImage;
        [SerializeField] private Button confirmButton;

        /// <summary>
        /// 行動が選択された時のコールバック
        /// </summary>
        public event Action<WifeActionModel> OnActionSelected;

        /// <summary>
        /// 行動が確定された時のコールバック
        /// </summary>
        public event Action<WifeActionModel> OnActionConfirmed;

        /// <summary>
        /// 戻るボタンが押された時のコールバック
        /// </summary>
        public event Action OnBackPressed;

        private List<Button> instantiatedButtons = new List<Button>();
        private WifeActionModel currentSelectedAction = null;
        private System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(256);

        // ヘッダーテキスト定数
        private const string COST_HEADER = "【コスト】\n";
        private const string EFFECTS_HEADER = "【効果】\n";
        private const string NONE_TEXT = "なし";

        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackPressed);
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(HandleConfirmPressed);
            }

            // 初期状態では非表示
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
            }

            // 詳細パネルも初期状態では非表示
            if (detailPanelRoot != null)
            {
                detailPanelRoot.SetActive(false);
            }
        }

        /// <summary>
        /// サブメニューを開き、指定されたカテゴリの行動リストを表示
        /// </summary>
        public void Open(List<WifeActionModel> actions)
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(true);
            }

            // 既存のボタンをクリア
            ClearButtons();

            // 行動リストからボタンを生成
            foreach (var action in actions)
            {
                CreateActionButton(action);
            }

            // 最初のアクションを自動選択
            if (actions != null && actions.Count > 0)
            {
                SelectAction(actions[0]);
            }
        }

        /// <summary>
        /// サブメニューを閉じる
        /// </summary>
        public void Close()
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
            }

            if (detailPanelRoot != null)
            {
                detailPanelRoot.SetActive(false);
            }

            ClearButtons();
            currentSelectedAction = null;
        }

        /// <summary>
        /// 行動選択ボタンを生成
        /// </summary>
        private void CreateActionButton(WifeActionModel action)
        {
            if (itemButtonPrefab == null || contentContainer == null)
            {
                return;
            }

            Button button = Instantiate(itemButtonPrefab, contentContainer);
            instantiatedButtons.Add(button);

            // ボタンのテキストを設定
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = action.Name;
            }

            // クリックイベントを設定 - 詳細パネルを更新するように変更
            button.onClick.AddListener(() => SelectAction(action));
        }

        /// <summary>
        /// 行動を選択して詳細パネルを更新
        /// </summary>
        public void SelectAction(WifeActionModel action)
        {
            if (action == null)
            {
                return;
            }

            currentSelectedAction = action;

            // 詳細パネルを表示
            if (detailPanelRoot != null)
            {
                detailPanelRoot.SetActive(true);
            }

            // アクション名
            if (detailNameText != null)
            {
                detailNameText.text = action.Name;
            }

            // コスト情報
            if (detailCostText != null)
            {
                detailCostText.text = BuildCostText(action);
            }

            // 効果情報
            if (detailEffectText != null)
            {
                detailEffectText.text = BuildEffectsText(action);
            }

            // 画像（将来的に実装される場合のため）
            // if (detailImage != null)
            // {
            //     detailImage.sprite = action.Image;
            // }

            // プレビュー表示を更新
            if (mainGameView != null)
            {
                mainGameView.ShowPreview(action.DeltaHP, action.DeltaMP, action.DeltaCOND, action.DeltaTeamEvaluation, action.DeltaLove, action.DeltaPublicEye);
            }

            // 従来のイベントも発火（互換性のため）
            OnActionSelected?.Invoke(action);
        }

        /// <summary>
        /// 戻るボタンが押された時の処理
        /// </summary>
        private void HandleBackPressed()
        {
            OnBackPressed?.Invoke();
        }

        /// <summary>
        /// 確定ボタンが押された時の処理
        /// </summary>
        private void HandleConfirmPressed()
        {
            if (currentSelectedAction != null)
            {
                OnActionConfirmed?.Invoke(currentSelectedAction);
            }
        }

        /// <summary>
        /// コスト情報のテキストを生成
        /// </summary>
        private string BuildCostText(WifeActionModel action)
        {
            stringBuilder.Clear();
            stringBuilder.Append(COST_HEADER);

            if (action.CashCost != 0)
            {
                stringBuilder.Append($"資金: {action.CashCost:N0}円\n");
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
        private string BuildEffectsText(WifeActionModel action)
        {
            stringBuilder.Clear();
            stringBuilder.Append(EFFECTS_HEADER);

            // 夫への効果
            if (action.DeltaHP != 0)
            {
                stringBuilder.Append($"夫体力: {FormatChangeValue(action.DeltaHP)}\n");
            }

            if (action.DeltaMP != 0)
            {
                stringBuilder.Append($"夫精神: {FormatChangeValue(action.DeltaMP)}\n");
            }

            if (action.DeltaCOND != 0)
            {
                stringBuilder.Append($"夫調子: {FormatChangeValue(action.DeltaCOND)}\n");
            }

            if (action.DeltaLove != 0)
            {
                stringBuilder.Append($"愛情: {FormatChangeValue(action.DeltaLove)}\n");
            }

            if (action.DeltaPublicEye != 0)
            {
                stringBuilder.Append($"世間の目: {FormatChangeValue(action.DeltaPublicEye)}\n");
            }

            if (action.DeltaTeamEvaluation != 0)
            {
                stringBuilder.Append($"チーム評価: {FormatChangeValue(action.DeltaTeamEvaluation)}\n");
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
        /// 生成されたボタンをすべて削除
        /// </summary>
        private void ClearButtons()
        {
            foreach (var button in instantiatedButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            instantiatedButtons.Clear();
        }

        private void OnDestroy()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HandleBackPressed);
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(HandleConfirmPressed);
            }
        }
    }
}
