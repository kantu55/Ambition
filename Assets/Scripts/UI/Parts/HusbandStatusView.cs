using Ambition.Data.Runtime;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI.MainGame.Parts
{
    /// <summary>
    /// 夫のステータス表示を管理するビューコンポーネント
    /// </summary>
    public class HusbandStatusView : MonoBehaviour
    {
        [Header("Husband UI")]
        [SerializeField] private Slider husbandHealthSlider;
        [SerializeField] private Slider husbandMentalSlider;
        [SerializeField] private TextMeshProUGUI husbandHealthText;
        [SerializeField] private TextMeshProUGUI husbandMentalText;
        [SerializeField] private TextMeshProUGUI husbandAgeText;
        [SerializeField] private TextMeshProUGUI husbandAbilityText;
        [SerializeField] private TextMeshProUGUI salaryText;

        // 文字列生成時のGC Allocを避けるためのStringBuilder
        private StringBuilder stringBuilder = new StringBuilder(512);

        /// <summary>
        /// 体力スライダーへのアクセス
        /// </summary>
        public Slider HealthSlider => husbandHealthSlider;

        /// <summary>
        /// 精神スライダーへのアクセス
        /// </summary>
        public Slider MentalSlider => husbandMentalSlider;

        /// <summary>
        /// 体力テキストへのアクセス
        /// </summary>
        public TextMeshProUGUI HealthText => husbandHealthText;

        /// <summary>
        /// 精神テキストへのアクセス
        /// </summary>
        public TextMeshProUGUI MentalText => husbandMentalText;

        /// <summary>
        /// 夫のステータス情報を更新
        /// </summary>
        /// <param name="husband">夫のステータス</param>
        public void UpdateHusbandInfo(RuntimePlayerStatus husband)
        {
            if (husband == null)
            {
                return;
            }

            // スライダー更新 (最大値に対する現在値)
            husbandHealthSlider.maxValue = (float)husband.MAX_HEALTH;
            husbandHealthSlider.value = husband.CurrentHealth;
            stringBuilder.Clear();
            stringBuilder.Append("体力: ").Append(husband.CurrentHealth).Append(" / ").Append(husband.MAX_HEALTH);
            husbandHealthText.SetText(stringBuilder);

            husbandMentalSlider.maxValue = (float)husband.MAX_MENTAL;
            husbandMentalSlider.value = husband.CurrentMental;
            stringBuilder.Clear();
            stringBuilder.Append("精神: ").Append(husband.CurrentMental).Append(" / ").Append(husband.MAX_MENTAL);
            husbandMentalText.SetText(stringBuilder);

            // 能力テキスト整形
            stringBuilder.Clear();
            stringBuilder.Append("選手能力: ").Append(husband.CurrentAbility);
            husbandAbilityText.SetText(stringBuilder);

            stringBuilder.Clear();
            stringBuilder.Append("年齢: ").Append(husband.CurrentAge).Append("歳");
            husbandAgeText.SetText(stringBuilder);

            // 契約金表示
            stringBuilder.Clear();
            stringBuilder.Append("当年契約: ¥");
            stringBuilder.Append(husband.Salary.ToString("N0"));
            salaryText.SetText(stringBuilder);
        }
    }
}
