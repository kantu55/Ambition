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

            // backButtonをcontentContainerの最後尾に移動
            if (backButton != null && contentContainer != null)
            {
                backButton.transform.SetParent(contentContainer);
                backButton.transform.SetAsLastSibling();
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

        private void OnDestroy()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HandleBackPressed);
            }
        }
    }
}
