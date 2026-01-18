using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI
{
    /// <summary>
    /// 汎用的な詳細パネル付きメニューコントローラーの基底クラス
    /// アクション選択や食事選択など、リスト表示+詳細表示+確定の流れを共通化
    /// </summary>
    /// <typeparam name="T">表示するアイテムの型</typeparam>
    public abstract class DetailPanelController<T> : MonoBehaviour where T : class
    {
        [Header("UI Components")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Transform contentContainer;
        [SerializeField] private Button itemButtonPrefab;
        [SerializeField] private Button backButton;

        [Header("Detail Panel Components")]
        [SerializeField] private GameObject detailPanelRoot;
        [SerializeField] private TMPro.TextMeshProUGUI detailNameText;
        [SerializeField] private TMPro.TextMeshProUGUI detailCostText;
        [SerializeField] private TMPro.TextMeshProUGUI detailEffectText;
        [SerializeField] private Image detailImage;
        [SerializeField] private Button confirmButton;

        /// <summary>
        /// アイテムが選択された時のコールバック
        /// </summary>
        public event Action<T> OnItemSelected;

        /// <summary>
        /// アイテムが確定された時のコールバック
        /// </summary>
        public event Action<T> OnItemConfirmed;

        /// <summary>
        /// 戻るボタンが押された時のコールバック
        /// </summary>
        public event Action OnBackPressed;

        protected List<Button> instantiatedButtons = new List<Button>();
        protected T currentSelectedItem = null;
        protected StringBuilder stringBuilder = new StringBuilder(256);

        protected const string COST_HEADER = "【コスト】\n";
        protected const string EFFECTS_HEADER = "【効果】\n";
        protected const string NONE_TEXT = "なし";
        protected const string CURRENCY_UNIT = "円";

        protected virtual void Awake()
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

            if (detailPanelRoot != null)
            {
                detailPanelRoot.SetActive(false);
            }
        }

        protected virtual void OnDestroy()
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

        /// <summary>
        /// メニューを開き、指定されたアイテムリストを表示
        /// </summary>
        public virtual void Open(List<T> items)
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(true);
            }

            // 既存のボタンをクリア
            ClearButtons();

            // アイテムリストからボタンを生成
            foreach (var item in items)
            {
                CreateItemButton(item);
            }

            // 最初のアイテムを自動選択
            if (items != null && items.Count > 0)
            {
                SelectItem(items[0], true);
            }
        }

        /// <summary>
        /// メニューを閉じる
        /// </summary>
        public virtual void Close()
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
            currentSelectedItem = null;
        }

        /// <summary>
        /// アイテム選択ボタンを生成
        /// </summary>
        protected virtual void CreateItemButton(T item)
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
                buttonText.text = GetItemName(item);
            }

            // クリックイベントを設定 - 詳細パネルを更新
            button.onClick.AddListener(() => SelectItem(item));
        }

        /// <summary>
        /// アイテムを選択して詳細パネルを更新
        /// </summary>
        public virtual void SelectItem(T item, bool isFirst = false)
        {
            if (item == null)
            {
                return;
            }

            currentSelectedItem = item;

            // 詳細パネルを表示
            if (detailPanelRoot != null)
            {
                detailPanelRoot.SetActive(true);
            }

            // アイテム名
            if (detailNameText != null)
            {
                detailNameText.text = GetItemName(item);
            }

            // コスト情報
            if (detailCostText != null)
            {
                detailCostText.text = BuildCostText(item);
            }

            // 効果情報
            if (detailEffectText != null)
            {
                detailEffectText.text = BuildEffectsText(item);
            }

            // 画像（サブクラスで実装）
            UpdateItemImage(item);

            // プレビュー表示を更新（サブクラスで実装）
            ShowPreview(item);

            if (isFirst == false)
            {
                OnItemSelected?.Invoke(item);
            }
        }

        /// <summary>
        /// 戻るボタンが押された時の処理
        /// </summary>
        protected virtual void HandleBackPressed()
        {
            OnBackPressed?.Invoke();
        }

        /// <summary>
        /// 確定ボタンが押された時の処理
        /// </summary>
        protected virtual void HandleConfirmPressed()
        {
            if (currentSelectedItem != null)
            {
                OnItemConfirmed?.Invoke(currentSelectedItem);
            }
        }

        /// <summary>
        /// 生成されたボタンをすべて削除
        /// </summary>
        protected virtual void ClearButtons()
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

        /// <summary>
        /// 数値変化を表示用の文字列に変換（正の値には+を付ける）
        /// </summary>
        protected string FormatChangeValue(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        // === サブクラスで実装する抽象メソッド ===

        /// <summary>
        /// アイテム名を取得
        /// </summary>
        protected abstract string GetItemName(T item);

        /// <summary>
        /// コスト情報のテキストを生成
        /// </summary>
        protected abstract string BuildCostText(T item);

        /// <summary>
        /// 効果情報のテキストを生成
        /// </summary>
        protected abstract string BuildEffectsText(T item);

        /// <summary>
        /// アイテム画像を更新
        /// </summary>
        protected abstract void UpdateItemImage(T item);

        /// <summary>
        /// プレビュー表示を更新
        /// </summary>
        protected abstract void ShowPreview(T item);
    }
}
