using Ambition.DataStructures;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ambition.UI
{
    /// <summary>
    /// サブメニュー（アクションリスト）を管理するコントローラー
    /// </summary>
    public class SubMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject subMenuPanel;
        [SerializeField] private Transform actionListContainer;
        [SerializeField] private GameObject actionItemPrefab;
        [SerializeField] private Button backButton;

        private List<WifeActionModel> currentActions;
        private Action<WifeActionModel> onActionSelected;
        private Action onBack;

        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }
        }

        /// <summary>
        /// サブメニューを開く
        /// </summary>
        /// <param name="actions">表示するアクションリスト</param>
        /// <param name="onSelected">アクション選択時のコールバック</param>
        /// <param name="onBackCallback">戻るボタン押下時のコールバック</param>
        public void Open(List<WifeActionModel> actions, Action<WifeActionModel> onSelected, Action onBackCallback = null)
        {
            currentActions = actions;
            onActionSelected = onSelected;
            onBack = onBackCallback;

            PopulateActionList();
            
            if (subMenuPanel != null)
            {
                subMenuPanel.SetActive(true);
            }
        }

        /// <summary>
        /// サブメニューを閉じる
        /// </summary>
        public void Close()
        {
            if (subMenuPanel != null)
            {
                subMenuPanel.SetActive(false);
            }

            ClearActionList();
        }

        /// <summary>
        /// アクションリストを生成
        /// </summary>
        private void PopulateActionList()
        {
            ClearActionList();

            if (currentActions == null || actionListContainer == null || actionItemPrefab == null)
            {
                return;
            }

            foreach (var action in currentActions)
            {
                GameObject itemObj = Instantiate(actionItemPrefab, actionListContainer);
                
                // アイテムのテキストを設定
                TextMeshProUGUI itemText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
                if (itemText != null)
                {
                    itemText.text = action.Name;
                }

                // ボタンのクリックイベントを設定
                Button itemButton = itemObj.GetComponent<Button>();
                if (itemButton != null)
                {
                    itemButton.onClick.AddListener(() => OnActionItemClicked(action));
                }
            }
        }

        /// <summary>
        /// アクションリストをクリア
        /// </summary>
        private void ClearActionList()
        {
            if (actionListContainer == null)
            {
                return;
            }

            foreach (Transform child in actionListContainer)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// アクションアイテムがクリックされた時の処理
        /// </summary>
        private void OnActionItemClicked(WifeActionModel action)
        {
            onActionSelected?.Invoke(action);
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
