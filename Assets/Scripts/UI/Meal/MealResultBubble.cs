using Ambition.DataStructures;
using Ambition.GameCore;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace Ambition.UI.Meal
{
    public class MealResultBubble : MonoBehaviour
    {
        // --- インスペクター設定 ---
        [Header("UI参照")]
        [SerializeField] private GameObject bubbleRoot;
        [SerializeField] private TextMeshProUGUI resultText;

        // --- 内部変数 ---
        private StringBuilder stringBuilder = new StringBuilder(256);

        private void Awake()
        {
            if (bubbleRoot != null)
            {
                bubbleRoot.SetActive(false);
            }
        }

        public void ShowResult(FoodMitModel menu)
        {
            if (menu == null)
            {
                return;
            }

            if (bubbleRoot != null)
            {
                bubbleRoot.SetActive(true);
            }

            if (resultText != null)
            {
                resultText.text = BuildResultText(menu);
            }
        }

        public void HideResult()
        {
            if (bubbleRoot != null)
            {
                bubbleRoot.SetActive(false);
            }
        }

        private string BuildResultText(FoodMitModel menu)
        {
            var list = DataManager.Instance.GetDatas<FoodModel>();
            var food = list.FirstOrDefault(v => v.Id.ToString() == menu.Tier);
            int cost = food != null ? food.MonthlyCost : 0;

            stringBuilder.Clear();
            stringBuilder.Append(menu.MenuName);
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
    }
}