using Ambition.GameCore;
using Ambition.RuntimeData;
using Ambition.DataStructures;
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
        [SerializeField] private TextMeshProUGUI wifeCookingLevelText;
        [SerializeField] private TextMeshProUGUI wifeCareLevelText;
        [SerializeField] private TextMeshProUGUI wifePRLevelText;
        [SerializeField] private TextMeshProUGUI wifeCaochLevelText;

        [Header("Command Buttons")]
        [SerializeField] private Button buttonCare;
        [SerializeField] private Button buttonSupport;
        [SerializeField] private Button buttonSNS;
        [SerializeField] private Button buttonDiscipline;
        [SerializeField] private Button buttonTalk;
        [SerializeField] private Button buttonRest;

        [Header("Confirm Button")]
        [SerializeField] private Button confirmButton;

        [Header("Action Info Panels")]
        [SerializeField] private ActionInfoPanel actionInfoCare;
        [SerializeField] private ActionInfoPanel actionInfoSupport;
        [SerializeField] private ActionInfoPanel actionInfoSNS;
        [SerializeField] private ActionInfoPanel actionInfoDiscipline;
        [SerializeField] private ActionInfoPanel actionInfoTalk;
        [SerializeField] private ActionInfoPanel actionInfoRest;

        [Header("Sub Controllers")]
        [SerializeField] private SubMenuController subMenuController;
        [SerializeField] private ActionDialogController actionDialogController;

        // 文字列生成時のGC Allocを避けるためのStringBuilder
        private StringBuilder stringBuilder = new StringBuilder(512);

        // --- プロパティ ---

        /// <summary>
        /// 確定ボタンへのアクセス
        /// </summary>
        public Button ConfirmButton => confirmButton;

        /// <summary>
        /// サブメニューコントローラーへのアクセス
        /// </summary>
        public SubMenuController SubMenuController => subMenuController;

        /// <summary>
        /// アクションダイアログコントローラーへのアクセス
        /// </summary>
        public ActionDialogController ActionDialogController => actionDialogController;

        /// <summary>
        /// ボタンクリック時のコールバックを登録
        /// </summary>
        public void BindButtons(UnityAction onCare, UnityAction onSupport, UnityAction onSNS, UnityAction onDiscipline, UnityAction onTalk, UnityAction onRest)
        {
            if (buttonCare != null)
            {
                buttonCare.onClick.AddListener(onCare);
            }

            if (buttonSupport != null)
            {
                buttonSupport.onClick.AddListener(onSupport);
            }

            if (buttonSNS != null)
            {
                buttonSNS.onClick.AddListener(onSNS);
            }

            if (buttonDiscipline != null)
            {
                buttonDiscipline.onClick.AddListener(onDiscipline);
            }

            if (buttonTalk != null)
            {
                buttonTalk.onClick.AddListener(onTalk);
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
            wifeCookingLevelText.SetText(GetWifeSkillText("料理: Lv", wife.CookingLevel));
            wifeCareLevelText.SetText(GetWifeSkillText("ケア: Lv", wife.CareLevel));
            wifePRLevelText.SetText(GetWifeSkillText("PR: Lv", wife.PRLevel));
            wifeCaochLevelText.SetText(GetWifeSkillText("コーチ: Lv", wife.CoachLevel));
        }

        private string GetWifeSkillText(string titleName, int level)
        {
            stringBuilder.Clear();
            stringBuilder.Append(titleName).Append(level);
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 選択されたアクションを表示
        /// </summary>
        /// <param name="action">選択されたアクション（nullの場合はクリア）</param>
        public void UpdateSelectedAction(WifeActionModel action)
        {
            if (action == null)
            {
                // すべてのアクション情報パネルを非表示
                HideAllActionInfo();
                return;
            }

            // まず全て非表示
            HideAllActionInfo();

            // アクションのカテゴリに応じて対応するパネルを表示
            ActionInfoPanel targetPanel = GetActionInfoPanelForCategory(action.GetMainCategory());
            if (targetPanel != null)
            {
                targetPanel.Show(action);
            }
        }

        /// <summary>
        /// すべてのアクション情報パネルを非表示にする
        /// </summary>
        private void HideAllActionInfo()
        {
            if (actionInfoCare != null)
            {
                actionInfoCare.Hide();
            }

            if (actionInfoSupport != null)
            {
                actionInfoSupport.Hide();
            }

            if (actionInfoSNS != null)
            {
                actionInfoSNS.Hide();
            }

            if (actionInfoDiscipline != null)
            {
                actionInfoDiscipline.Hide();
            }

            if (actionInfoTalk != null)
            {
                actionInfoTalk.Hide();
            }

            if (actionInfoRest != null)
            {
                actionInfoRest.Hide();
            }
        }

        /// <summary>
        /// アクションのカテゴリに対応するアクション情報パネルを取得
        /// </summary>
        private ActionInfoPanel GetActionInfoPanelForCategory(WifeActionModel.ActionMainCategory category)
        {
            switch (category)
            {
                case WifeActionModel.ActionMainCategory.CARE:
                    return actionInfoCare;
                case WifeActionModel.ActionMainCategory.SUPPORT:
                    return actionInfoSupport;
                case WifeActionModel.ActionMainCategory.SNS:
                    return actionInfoSNS;
                case WifeActionModel.ActionMainCategory.DISCIPLINE:
                    return actionInfoDiscipline;
                case WifeActionModel.ActionMainCategory.TALK:
                    return actionInfoTalk;
                case WifeActionModel.ActionMainCategory.REST:
                    return actionInfoRest;
                default:
                    return null;
            }
        }
    }
}
