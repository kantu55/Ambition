using Ambition.DataStructures;
using Ambition.GameCore;
using Ambition.RuntimeData;
using System.Collections.Generic;
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
        [SerializeField] private MealSelectionPanel selectionPanel;
        [SerializeField] private MealConfirmPanel confirmPanel;
        [SerializeField] private MealResultBubble resultBubble;

        [Header("Main Button")]
        [SerializeField] private Button mealMenuButton;

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
            Confirmation
        }

        private MealFlowState currentState = MealFlowState.Hidden;

        private void Awake()
        {
            // ボタンのイベントリスナー登録
            if (mealMenuButton != null)
            {
                mealMenuButton.onClick.AddListener(OnMealMenuButtonClicked);
            }

            // パネルのイベントリスナー登録
            if (tierPanel != null)
            {
                tierPanel.OnTierSelected += OnTierSelected;
                tierPanel.OnBackPressed += OnTierBackPressed;
            }

            if (selectionPanel != null)
            {
                selectionPanel.OnMenuSelected += OnMenuSelected;
                selectionPanel.OnBackPressed += OnSelectionBackPressed;
            }

            if (confirmPanel != null)
            {
                confirmPanel.OnConfirmPressed += OnConfirmPressed;
                confirmPanel.OnBackPressed += OnConfirmBackPressed;
            }

            // 初期状態では全て非表示
            HideAllPanels();
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            // イベントリスナーのクリーンアップ
            if (mealMenuButton != null)
            {
                mealMenuButton.onClick.RemoveListener(OnMealMenuButtonClicked);
            }

            if (tierPanel != null)
            {
                tierPanel.OnTierSelected -= OnTierSelected;
                tierPanel.OnBackPressed -= OnTierBackPressed;
            }

            if (selectionPanel != null)
            {
                selectionPanel.OnMenuSelected -= OnMenuSelected;
                selectionPanel.OnBackPressed -= OnSelectionBackPressed;
            }

            if (confirmPanel != null)
            {
                confirmPanel.OnConfirmPressed -= OnConfirmPressed;
                confirmPanel.OnBackPressed -= OnConfirmBackPressed;
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
        }

        /// <summary>
        /// "食事メニュー"ボタンがクリックされた時の処理
        /// </summary>
        private void OnMealMenuButtonClicked()
        {
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
        /// ティア選択画面で"戻る"ボタンが押された時の処理
        /// </summary>
        private void OnTierBackPressed()
        {
            // ホームに戻る（全パネルを非表示にする）
            HideAllPanels();
            currentState = MealFlowState.Hidden;
        }

        /// <summary>
        /// ②食事_メニュー選択 を表示
        /// </summary>
        private void ShowMenuSelection(string tier)
        {
            currentState = MealFlowState.MenuSelection;

            HideAllPanels();

            if (selectionPanel != null)
            {
                // 選択されたティアのメニューのみをフィルタリング
                List<FoodMitModel> menusForTier = foodMitModels.FindAll(m => m.Tier == tier);
                selectionPanel.Show(menusForTier);
            }
        }

        /// <summary>
        /// メニューが選択された時の処理
        /// </summary>
        private void OnMenuSelected(FoodMitModel menu)
        {
            currentSelectedMenu = menu;
            ShowConfirmation(menu);
        }

        /// <summary>
        /// メニュー選択画面で"戻る"ボタンが押された時の処理
        /// </summary>
        private void OnSelectionBackPressed()
        {
            // ティア選択に戻る
            ShowTierSelection();
        }

        /// <summary>
        /// ③食事_確認ダイアログ を表示
        /// </summary>
        private void ShowConfirmation(FoodMitModel menu)
        {
            currentState = MealFlowState.Confirmation;

            HideAllPanels();

            if (confirmPanel != null)
            {
                confirmPanel.Show(menu);
            }
        }

        /// <summary>
        /// 確認画面で"確認"ボタンが押された時の処理
        /// </summary>
        private void OnConfirmPressed(FoodMitModel menu)
        {
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
        private void OnConfirmBackPressed()
        {
            // メニュー選択に戻る
            ShowMenuSelection(currentSelectedTier);
        }

        /// <summary>
        /// すべてのパネルを非表示にする
        /// </summary>
        private void HideAllPanels()
        {
            if (tierPanel != null)
            {
                tierPanel.Hide();
            }

            if (selectionPanel != null)
            {
                selectionPanel.Hide();
            }

            if (confirmPanel != null)
            {
                confirmPanel.Hide();
            }
        }
    }
}