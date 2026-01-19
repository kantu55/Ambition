using Ambition.Data.Master;
using Ambition.Data.Runtime;
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
        [SerializeField] private Button tierButtonPrefab;

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
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
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

            ClearButtons();
        }

        /// <summary>
        /// Tier 1-4 のボタンを生成
        /// </summary>
        private void CreateTierButtons(List<FoodModel> foodModels)
        {
            if (tierButtonPrefab == null || panelRoot == null)
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
            Button button = Instantiate(tierButtonPrefab, panelRoot.transform);
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
        /// ティアが選択された時の処理
        /// </summary>
        private void HandleTierSelected(string tier)
        {
            OnTierSelected?.Invoke(tier);
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