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
        [Header("UI Components")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Transform contentContainer;
        [SerializeField] private Button itemButtonPrefab;
        [SerializeField] private Button backButton;

        /// <summary>
        /// 行動が選択された時のコールバック
        /// </summary>
        public event Action<WifeActionModel> OnActionSelected;

        /// <summary>
        /// 戻るボタンが押された時のコールバック
        /// </summary>
        public event Action OnBackPressed;

        private List<Button> instantiatedButtons = new List<Button>();

        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackPressed);
            }

            // 初期状態では非表示
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
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

            // 生成されたボタンの数に応じて背景のサイズを調整
            AdjustBackgroundSize();
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

            ClearButtons();
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

            // クリックイベントを設定
            button.onClick.AddListener(() => HandleActionSelected(action));
        }

        /// <summary>
        /// 行動が選択された時の処理
        /// </summary>
        private void HandleActionSelected(WifeActionModel action)
        {
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

        /// <summary>
        /// 生成されたボタンの数に応じて背景のサイズを調整する
        /// </summary>
        private void AdjustBackgroundSize()
        {
            if (menuPanel == null || contentContainer == null || instantiatedButtons.Count == 0)
            {
                return;
            }

            RectTransform panelRect = menuPanel.GetComponent<RectTransform>();
            if (panelRect == null)
            {
                return;
            }

            // ピボットをY=0（下）に設定して、上に伸びるようにする
            Vector2 pivot = panelRect.pivot;
            pivot.y = 0f;
            panelRect.pivot = pivot;

            // ボタンの高さを取得（全ボタンが同じプレハブから生成されるため、最初のボタンの高さを使用）
            float buttonHeight = 0f;
            if (instantiatedButtons.Count > 0 && instantiatedButtons[0] != null)
            {
                RectTransform buttonRect = instantiatedButtons[0].GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    // 実際にレンダリングされた高さを使用（Content Size Fitterなどのレイアウト制御に対応）
                    buttonHeight = buttonRect.rect.height;
                }
            }

            // ボタンの高さが取得できない場合は処理を中断
            if (buttonHeight <= 0f)
            {
                return;
            }

            // VerticalLayoutGroupの設定を取得
            float spacing = 0f;
            float padding = 0f;
            VerticalLayoutGroup layoutGroup = contentContainer.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                spacing = layoutGroup.spacing;
                padding = layoutGroup.padding.top + layoutGroup.padding.bottom;
            }

            // 背景の高さを計算: ボタンの高さ * 個数 + スペーシング * (個数-1) + パディング
            float totalHeight = (buttonHeight * instantiatedButtons.Count) + (spacing * (instantiatedButtons.Count - 1)) + padding;

            // 背景のサイズを適用
            Vector2 sizeDelta = panelRect.sizeDelta;
            sizeDelta.y = totalHeight;
            panelRect.sizeDelta = sizeDelta;
        }

        private void OnDestroy()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HandleBackPressed);
            }
        }
    }
}
