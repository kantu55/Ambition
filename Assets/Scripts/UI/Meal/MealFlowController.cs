using Ambition.Data.Master;
using Ambition.Data.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI.Meal
{
    /// <summary>
    /// 食事機能の画面フローを管理するメインコントローラー
    /// ①食事_ティア選択 → ②食事_メニュー選択 → ③食事_確認ダイアログ の流れを制御
    /// </summary>
    public class MealFlowController : MonoBehaviour
    {
        [SerializeField] private MainGameView mainGameView;

        [Header("UI Panels")]
        [SerializeField] private MealTierPanel tierPanel;
        [SerializeField] private MealDetailPanelController mealDetailPanel;
        [SerializeField] private MealConfirmPanel confirmPanel;
        [SerializeField] private MealResultBubble resultBubble;

        private RuntimeFixedCost runtimeFixedCost;
        private List<FoodModel> foodModels;
        private List<FoodMitModel> foodMitModels;

        private string currentSelectedTier;
        private FoodMitModel currentSelectedMenu;

        /// <summary>
        /// 食事機能の状態
        /// </summary>
        private enum MealFlowState
        {
            Hidden,
            TierSelection,
            MenuSelection,
        }

        private MealFlowState currentState = MealFlowState.Hidden;

        private void Awake()
        {
            Initialize();

            // パネルのイベントリスナー登録
            if (tierPanel != null)
            {
                tierPanel.OnTierSelected += OnTierSelected;
            }

            if (mealDetailPanel != null)
            {
                mealDetailPanel.OnMenuSelected += OnMenuConfirmed;
                mealDetailPanel.OnBackPressed += OnSelectionBackPressed;
            }

            // 初期状態では全て非表示
            HideAllPanels();
        }

        private void OnDestroy()
        {
            if (tierPanel != null)
            {
                tierPanel.OnTierSelected -= OnTierSelected;
            }

            if (mealDetailPanel != null)
            {
                mealDetailPanel.OnMenuSelected -= OnMenuConfirmed;
                mealDetailPanel.OnBackPressed -= OnSelectionBackPressed;
            }
        }

        /// <summary>
        /// RuntimeFixedCostの参照を設定
        /// </summary>
        public void SetRuntimeFixedCost(RuntimeFixedCost fixedCost)
        {
            runtimeFixedCost = fixedCost;
        }

        /// <summary>
        /// データを読み込んで初期化
        /// </summary>
        public void Initialize()
        {
            // DataManagerからFoodModelとFoodMitModelを取得
            if (DataManager.Instance != null)
            {
                foodModels = DataManager.Instance.GetDatas<FoodModel>();
                foodMitModels = DataManager.Instance.GetDatas<FoodMitModel>();
            }
            else
            {
                Debug.LogWarning("[MealFlowController] DataManager.Instance is null");
                foodModels = new List<FoodModel>();
                foodMitModels = new List<FoodMitModel>();
            }
            ShowTierSelection();
        }

        /// <summary>
        /// ①食事_ティア選択 を表示
        /// </summary>
        private void ShowTierSelection()
        {
            currentState = MealFlowState.TierSelection;

            HideAllPanels();

            if (tierPanel != null)
            {
                tierPanel.gameObject.SetActive(true);
                tierPanel.Show(foodModels, runtimeFixedCost);
            }
        }

        /// <summary>
        /// ティアが選択された時の処理
        /// </summary>
        private void OnTierSelected(string tier)
        {
            currentSelectedTier = tier;
            ShowMenuSelection(tier);
        }

        /// <summary>
        /// ②食事_メニュー選択 を表示
        /// </summary>
        private void ShowMenuSelection(string tier)
        {
            currentState = MealFlowState.MenuSelection;

            HideAllPanels();

            if (mealDetailPanel != null)
            {
                // 選択されたティアのメニューのみをフィルタリング
                List<FoodMitModel> menusForTier = foodMitModels.FindAll(m => m.Tier == tier);
                mealDetailPanel.Show(menusForTier);
            }
        }

        /// <summary>
        /// メニューが選択された時の処理
        /// </summary>
        private void OnMenuConfirmed(FoodMitModel menu)
        {
            currentSelectedMenu = menu;

            // 結果バブルを表示
            if (resultBubble != null)
            {
                resultBubble.ShowResult(menu);
            }

            if (mainGameView != null)
            {
                mainGameView.UpdateSelectedMenu(menu);
            }

            // ここで実際の食事処理を実行する（コストの適用など）
            // 例: RuntimeFixedCostの更新、プレイヤーステータスへの効果適用など
            Debug.Log($"[MealFlowController] 食事を実行: {menu.MenuName}");

            // 全パネルを閉じる
            HideAllPanels();
            currentState = MealFlowState.Hidden;
        }

        /// <summary>
        /// 確認画面で"戻る"ボタンが押された時の処理
        /// </summary>
        private void OnSelectionBackPressed()
        {
        }

        /// <summary>
        /// すべてのパネルを非表示にする
        /// </summary>
        private void HideAllPanels()
        {
            if (mealDetailPanel != null)
            {
                mealDetailPanel.Hide();
            }

            if (confirmPanel != null)
            {
                confirmPanel.Hide();
            }
        }
    }
}