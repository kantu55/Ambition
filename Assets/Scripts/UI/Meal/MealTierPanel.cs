using Ambition.DataStructures;
using Ambition.RuntimeData;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI.Meal
{
    /// <summary>
    /// ①食事_ティア選択 のパネル
    /// Tier 1-4 のサブメニューと食費情報を表示
    /// </summary>
    public class MealTierPanel : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform tierButtonContainer;
        [SerializeField] private Button tierButtonPrefab;
        [SerializeField] private Button backButton;

        [Header("Food Expenses Panel")]
        [SerializeField] private GameObject expensesPanelRoot;
        [SerializeField] private TextMeshProUGUI expensesText;

        /// <summary>
        /// ティアが選択された時のコールバック
        /// </summary>
        public event Action<string> OnTierSelected;

        /// <summary>
        /// 戻るボタンが押された時のコールバック
        /// </summary>
        public event Action OnBackPressed;

        private List<Button> instantiatedButtons = new List<Button>();
        private StringBuilder stringBuilder = new StringBuilder(256);

        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackPressed);
            }

            // 初期状態では非表示
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (expensesPanelRoot != null)
            {
                expensesPanelRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HandleBackPressed);
            }
        }

        /// <summary>
        /// ティア選択パネルを表示
        /// </summary>
        public void Show(List<FoodModel> foodModels, RuntimeFixedCost fixedCost)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            // 既存のボタンをクリア
            ClearButtons();

            // Tier 1-4 のボタンを生成
            CreateTierButtons(foodModels);

            // 食費情報を表示
            ShowExpenses(fixedCost);
        }

        /// <summary>
        /// ティア選択パネルを非表示
        /// </summary>
        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (expensesPanelRoot != null)
            {
                expensesPanelRoot.SetActive(false);
            }

            ClearButtons();
        }

        /// <summary>
        /// Tier 1-4 のボタンを生成
        /// </summary>
        private void CreateTierButtons(List<FoodModel> foodModels)
        {
            if (tierButtonPrefab == null || tierButtonContainer == null)
            {
                Debug.LogWarning("[MealTierPanel] tierButtonPrefab or tierButtonContainer is null");
                return;
            }

            // Tier 0-4 のボタンを生成
            for (int tier = 0; tier <= 4; tier++)
            {
                stringBuilder.Clear();
                stringBuilder.Append("Q");
                stringBuilder.Append(tier);
                string tierString = stringBuilder.ToString();
                CreateTierButton(tierString, foodModels);
            }
        }

        /// <summary>
        /// 個別のティアボタンを生成
        /// </summary>
        private void CreateTierButton(string tier, List<FoodModel> foodModels)
        {
            Button button = Instantiate(tierButtonPrefab, tierButtonContainer);
            instantiatedButtons.Add(button);

            // ボタンのテキストを設定
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                // FoodModelからティアに対応する名前を取得
                FoodModel foodModel = foodModels.Find(f => f.Tier == tier);
                if (foodModel != null)
                {
                    buttonText.text = foodModel.Name;
                }
                else
                {
                    buttonText.text = $"Tier {tier}";
                }
            }

            // クリックイベントを設定
            button.onClick.AddListener(() => HandleTierSelected(tier));
        }

        /// <summary>
        /// 食費情報を表示
        /// </summary>
        private void ShowExpenses(RuntimeFixedCost fixedCost)
        {
            if (expensesPanelRoot != null)
            {
                expensesPanelRoot.SetActive(true);
            }

            if (expensesText != null && fixedCost != null)
            {
                stringBuilder.Clear();
                stringBuilder.Append("【食費】\n");
                stringBuilder.Append("現在の食費: ¥");
                stringBuilder.Append(fixedCost.FoodCost.ToString("N0"));
                stringBuilder.Append(" / 月");
                expensesText.text = stringBuilder.ToString();
            }
        }

        /// <summary>
        /// ティアが選択された時の処理
        /// </summary>
        private void HandleTierSelected(string tier)
        {
            OnTierSelected?.Invoke(tier);
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
    }
}