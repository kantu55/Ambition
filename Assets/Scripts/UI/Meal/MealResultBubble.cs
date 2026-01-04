using Ambition.DataStructures;
using System.Text;
using TMPro;
using UnityEngine;

namespace Ambition.UI.Meal
{
    /// <summary>
    /// 食事選択後の結果を表示するバブル（ポップアップ）
    /// メインボタンの上に選択した食事情報を表示
    /// </summary>
    public class MealResultBubble : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject bubbleRoot;
        [SerializeField] private TextMeshProUGUI resultText;

        [Header("Display Settings")]
        [SerializeField] private float displayDuration = 3.0f;

        private StringBuilder stringBuilder = new StringBuilder(256);
        private float displayTimer = 0f;
        private bool isDisplaying = false;

        private void Awake()
        {
            // 初期状態では非表示
            if (bubbleRoot != null)
            {
                bubbleRoot.SetActive(false);
            }
        }

        private void Update()
        {
            // 表示タイマーの更新
            if (isDisplaying)
            {
                displayTimer -= Time.deltaTime;
                if (displayTimer <= 0f)
                {
                    Hide();
                }
            }
        }

        /// <summary>
        /// 結果バブルを表示
        /// </summary>
        public void Show(FoodMitModel menu)
        {
            if (menu == null)
            {
                Debug.LogWarning("[MealResultBubble] menu is null");
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

            // 表示タイマーを設定
            displayTimer = displayDuration;
            isDisplaying = true;
        }

        /// <summary>
        /// 結果バブルを非表示
        /// </summary>
        public void Hide()
        {
            if (bubbleRoot != null)
            {
                bubbleRoot.SetActive(false);
            }

            isDisplaying = false;
            displayTimer = 0f;
        }

        /// <summary>
        /// 結果テキストを生成
        /// </summary>
        private string BuildResultText(FoodMitModel menu)
        {
            stringBuilder.Clear();
            stringBuilder.Append("【食事を摂りました】\n");
            stringBuilder.Append(menu.MenuName);
            stringBuilder.Append("\n\n");

            stringBuilder.Append("【効果】\n");

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

            if (stringBuilder.ToString().EndsWith("【効果】\n"))
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
