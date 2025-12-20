using Ambition.GameCore;
using Ambition.RuntimeData;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Ambition.UI
{
    /// <summary>
    /// メイン画面のUI表示を管理するクラス
    /// </summary>
    public class MainGameView : MonoBehaviour
    {
        [Header("Global Info")]
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private TextMeshProUGUI moneyText;

        [Header("Husband UI")]
        [SerializeField] private Slider husbandHealthSlider;
        [SerializeField] private Slider husbandMentalSlider;
        [SerializeField] private TextMeshProUGUI husbandAbilityText; // 筋力などの一覧表示用

        [Header("Wife UI")]
        [SerializeField] private Slider wifeStaminaSlider;
        [SerializeField] private TextMeshProUGUI wifeSkillText; // スキル一覧表示用

        [Header("Command Buttons")]
        [SerializeField] private Button buttonSupport;
        [SerializeField] private Button buttonSelfPolish;
        [SerializeField] private Button buttonEnvironment;
        [SerializeField] private Button buttonPR;

        // 文字列生成時のGC Allocを避けるためのStringBuilder
        private StringBuilder stringBuilder = new StringBuilder(512);

        /// <summary>
        /// ボタンクリック時のコールバックを登録
        /// </summary>
        public void BindButtons(UnityAction onSupport, UnityAction onSelfPolish, UnityAction onEnvironment, UnityAction onPR)
        {
            if (buttonSupport != null)
            {
                buttonSupport.onClick.AddListener(onSupport);
            }

            if (buttonSelfPolish != null)
            {
                buttonSelfPolish.onClick.AddListener(onSelfPolish);
            }

            if (buttonEnvironment != null)
            {
                buttonEnvironment.onClick.AddListener(onEnvironment);
            }
            
            if (buttonPR != null)
            {
                buttonPR.onClick.AddListener(onPR);
            }
        }

        /// <summary>
        /// 全体の表示を更新
        /// </summary>
        public void RefreshView(RuntimeDate date, RuntimeHouseholdBudget budget, RuntimePlayerStatus husband, RuntimeWifeStatus wife)
        {
            UpdateGlobalInfo(date, budget);
            UpdateHusbandInfo(husband);
            UpdateWifeInfo(wife);
        }

        private void UpdateGlobalInfo(RuntimeDate date, RuntimeHouseholdBudget budget)
        {
            dateText.text = $"{date.Year}年\n{date.Month}月";
            moneyText.text = budget.CurrentSavings.ToString("N0");
        }

        private void UpdateHusbandInfo(RuntimePlayerStatus husband)
        {
            if (husband == null)
            {
                return;
            }

            // スライダー更新 (最大値に対する現在値)
            husbandHealthSlider.maxValue = (float)husband.MAX_HEALTH;
            husbandHealthSlider.value = husband.CurrentHealth;

            // マジックナンバー(100)を定数に置き換え
            husbandMentalSlider.maxValue = (float)husband.MAX_MENTAL;
            husbandMentalSlider.value = husband.CurrentMental;

            // 能力テキスト整形
            stringBuilder.Clear();
            stringBuilder.Append("筋力: ").Append(husband.Muscle).Append('\n');
            stringBuilder.Append("技術: ").Append(husband.Technique).Append('\n');
            stringBuilder.Append("集中: ").Append(husband.Concentration).Append('\n');
            stringBuilder.Append("評価: ").Append(husband.Evaluation);
            husbandAbilityText.SetText(stringBuilder);
        }

        private void UpdateWifeInfo(RuntimeWifeStatus wife)
        {
            if (wife == null)
            {
                return;
            }


            // スキル一覧
            stringBuilder.Clear();
            stringBuilder.Append("料理: Lv").Append(wife.CookingLevel).Append('\n');
            stringBuilder.Append("容姿: Lv").Append(wife.LooksLevel).Append('\n');
            stringBuilder.Append("マーケティング: Lv").Append(wife.SocialLevel);
            wifeSkillText.SetText(stringBuilder);

            wifeStaminaSlider.maxValue = wife.MaxHealth;
            wifeStaminaSlider.value = wife.CurrentHealth;
        }
    }
}
