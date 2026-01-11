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
            stringBuilder.Append(menu.MenuName).Append("\n");
            stringBuilder.Append("ティア: ").Append(menu.Tier).Append("\n");
            stringBuilder.Append("食費: ").Append(cost).Append("\n");
            stringBuilder.Append("\n");

            const string effectHeader = "【効果】\n";
            int effectHeaderStart = stringBuilder.Length;
            stringBuilder.Append(effectHeader);

            if (menu.MitigHP != 0)
            {
                stringBuilder.Append($"体力回復: {FormatChangeValue(menu.MitigHP)}\n");
            }

            if (menu.MitigMP != 0)
            {
                stringBuilder.Append($"精神回復: {FormatChangeValue(menu.MitigMP)}\n");
            }

            if (menu.MitigCOND != 0)
            {
                stringBuilder.Append($"調子改善: {FormatChangeValue(menu.MitigCOND)}\n");
            }

            if (stringBuilder.Length == effectHeaderStart + effectHeader.Length)
            {
                stringBuilder.Append("なし");
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
    }
}