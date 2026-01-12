using Ambition.GameCore;
using Ambition.RuntimeData;
using Ambition.DataStructures;
using Cysharp.Threading.Tasks;
using System.Text;
using System.Threading;
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
        [SerializeField] private TextMeshProUGUI totalMoneyText;
        [SerializeField] private TextMeshProUGUI fixedCostText;
        [SerializeField] private TextMeshProUGUI salaryText;

        [Header("Reputation UI")]
        [SerializeField] private TextMeshProUGUI loveText;
        [SerializeField] private TextMeshProUGUI teamEvaluationText;
        [SerializeField] private TextMeshProUGUI publicEyeText;

        [Header("Husband UI")]
        [SerializeField] private Slider husbandHealthSlider;
        [SerializeField] private Slider husbandMentalSlider;
        [SerializeField] private TextMeshProUGUI husbandHealthText;
        [SerializeField] private TextMeshProUGUI husbandMentalText;
        [SerializeField] private TextMeshProUGUI husbandAgeText;
        [SerializeField] private TextMeshProUGUI husbandAbilityText;

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

        [Header("Preview UI")]

        // --- プレビュー用スライダー ---

        [SerializeField] private Slider husbandHealthPreviewSlider;
        [SerializeField] private Slider husbandMentalPreviewSlider;

        // --- プレビュー用テキスト ---

        [SerializeField] private TextMeshProUGUI husbandHealthPreviewText;
        [SerializeField] private TextMeshProUGUI husbandMentalPreviewText;

        // --- 増減を表す矢印表示用テキスト ---

        [SerializeField] private TextMeshProUGUI conditionArrowText;
        [SerializeField] private TextMeshProUGUI evaluationArrowText;
        [SerializeField] private TextMeshProUGUI loveArrowText;
        [SerializeField] private TextMeshProUGUI publicEyeArrowText;

        // --- 色設定 ---

        /// <summary>
        /// 増加時の色
        /// </summary>
        [SerializeField] private Color increaseColor = Color.green;

        /// <summary>
        /// 減少時の色
        /// </summary>
        [SerializeField] private Color decreaseColor = Color.red;

        // 文字列生成時のGC Allocを避けるためのStringBuilder
        private StringBuilder stringBuilder = new StringBuilder(512);

        // --- 行動による変動値のキャッシュ ---
        private int cachedActionDeltaHP = 0;
        private int cachedActionDeltaMP = 0;
        private int cachedActionDeltaCond = 0;
        private int cachedActionDeltaEval = 0;
        private int cachedActionDeltaLove = 0;
        private int cachedActionDeltaPublicEye = 0;
        private int cachedActionDeltaAbility = 0;

        // --- プレビュー点滅制御用 ---
        private const float BLINK_CYCLE = 1.0f; // 点滅周期（秒）
        private const float MIN_BLINK_ALPHA = 0.5f; // 点滅時の最小アルファ値
        private const float MAX_BLINK_ALPHA = 1.0f; // 点滅時の最大アルファ値
        private Image husbandHealthPreviewFillImage;
        private Image husbandMentalPreviewFillImage;
        private CancellationTokenSource blinkCancellationTokenSource;

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
        /// コンポーネント破棄時の処理
        /// </summary>
        private void OnDestroy()
        {
            StopBlinking();
        }

        /// <summary>
        /// プレビュー点滅処理（UniTask版）
        /// </summary>
        private async UniTaskVoid StartBlinkingAsync()
        {
            // 既存の点滅処理をキャンセル
            StopBlinking();
            
            // 新しいCancellationTokenSourceを作成
            blinkCancellationTokenSource = new CancellationTokenSource();
            var token = blinkCancellationTokenSource.Token;

            float elapsedTime = 0f;

            while (!token.IsCancellationRequested)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(MIN_BLINK_ALPHA, MAX_BLINK_ALPHA, (Mathf.Sin(elapsedTime * Mathf.PI * 2f / BLINK_CYCLE) + 1f) * 0.5f);

                // HP プレビューの点滅
                if (husbandHealthPreviewSlider != null && husbandHealthPreviewSlider.gameObject.activeSelf)
                {
                    InitializeImageCacheIfNeeded(husbandHealthPreviewSlider, ref husbandHealthPreviewFillImage);
                    if (husbandHealthPreviewFillImage != null)
                    {
                        Color color = husbandHealthPreviewFillImage.color;
                        color.a = alpha;
                        husbandHealthPreviewFillImage.color = color;
                    }
                }

                // MP プレビューの点滅
                if (husbandMentalPreviewSlider != null && husbandMentalPreviewSlider.gameObject.activeSelf)
                {
                    InitializeImageCacheIfNeeded(husbandMentalPreviewSlider, ref husbandMentalPreviewFillImage);
                    if (husbandMentalPreviewFillImage != null)
                    {
                        Color color = husbandMentalPreviewFillImage.color;
                        color.a = alpha;
                        husbandMentalPreviewFillImage.color = color;
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        /// <summary>
        /// プレビュー点滅処理を停止
        /// </summary>
        private void StopBlinking()
        {
            var cts = blinkCancellationTokenSource;
            if (cts != null)
            {
                blinkCancellationTokenSource = null;
                cts.Cancel();
                cts.Dispose();
            }
        }

        /// <summary>
        /// Image コンポーネントをキャッシュ（未キャッシュの場合のみ）
        /// </summary>
        private void InitializeImageCacheIfNeeded(Slider slider, ref Image cachedImage)
        {
            if (cachedImage == null && slider != null && slider.fillRect != null)
            {
                cachedImage = slider.fillRect.GetComponent<Image>();
            }
        }

        /// <summary>
        /// 全体の表示を更新
        /// </summary>
        public void RefreshView(RuntimeDate date, RuntimeHouseholdBudget budget, RuntimePlayerStatus husband, RuntimeWifeStatus wife, RuntimeReputation reputation)
        {
            UpdateGlobalInfo(date, budget);
            UpdateHusbandInfo(husband);
            UpdateWifeInfo(wife);
            UpdateEvaluationInfo(reputation);
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

                cachedActionDeltaHP = 0;
                cachedActionDeltaMP = 0;
                cachedActionDeltaCond = 0;
                cachedActionDeltaEval = 0;
                cachedActionDeltaLove = 0;
                cachedActionDeltaPublicEye = 0;
                cachedActionDeltaAbility = 0;
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

            cachedActionDeltaHP = action.DeltaHP;
            cachedActionDeltaMP = action.DeltaMP;
            cachedActionDeltaCond = action.DeltaCOND;
            cachedActionDeltaEval = action.DeltaTeamEvaluation;
            cachedActionDeltaLove = action.DeltaLove;
            cachedActionDeltaPublicEye = action.DeltaPublicEye;
            cachedActionDeltaAbility = 0;
        }

        /// <summary>
        /// 行動・食事の効果をプレビュー表示
        /// </summary>
        /// <param name="deltaHP">体力の増減値</param>
        /// <param name="deltaMP">精神の増減値</param>
        /// <param name="deltaCond">調子の増減値</param>
        /// <param name="deltaEval">評価の増減値</param>
        public void ShowPreview(int deltaHP, int deltaMP, int deltaCond, int deltaEval = 0, int deltaLove = 0, int deltaPublicEye = 0, int deltaAbility = 0)
        {
            RuntimePlayerStatus currentStatus = GameSimulationManager.Instance.Husband;
            if (currentStatus == null)
            {
                return;
            }

            int totalDeltaHP = cachedActionDeltaHP + deltaHP;
            int totalDeltaMP = cachedActionDeltaMP + deltaMP;
            int totalDeltaCond = cachedActionDeltaCond + deltaCond;
            int totalDeltaEval = cachedActionDeltaEval + deltaEval;
            int totalDeltaLove = cachedActionDeltaLove + deltaLove;
            int totalDeltaPublicEye = cachedActionDeltaPublicEye + deltaPublicEye;

            UpdatePreviewSlider(
                husbandHealthSlider,
                husbandHealthPreviewSlider,
                husbandHealthPreviewText,
                currentStatus.CurrentHealth,
                currentStatus.MAX_HEALTH,
                totalDeltaHP);

            UpdatePreviewSlider(
                husbandMentalSlider,
                husbandMentalPreviewSlider,
                husbandMentalPreviewText,
                currentStatus.CurrentMental,
                currentStatus.MAX_MENTAL,
                totalDeltaMP);

            UpdateArrowText(conditionArrowText, totalDeltaCond);
            UpdateArrowText(evaluationArrowText, totalDeltaEval);
            UpdateArrowText(loveArrowText, totalDeltaLove);
            UpdateArrowText(publicEyeArrowText, totalDeltaPublicEye);

            // プレビュー点滅処理を開始
            StartBlinkingAsync().Forget();
        }

        /// <summary>
        /// プレビュー表示を非表示にする
        /// </summary>
        public void HidePreview()
        {
            // キャッシュされた増減値がある場合は、それをプレビュー表示し続ける
            if (cachedActionDeltaHP != 0 || cachedActionDeltaMP != 0 || cachedActionDeltaCond != 0 || 
                cachedActionDeltaEval != 0 || cachedActionDeltaLove != 0 || cachedActionDeltaPublicEye != 0 || 
                cachedActionDeltaAbility != 0)
            {
                // キャッシュ分のプレビューを表示
                ShowPreview(0, 0, 0, 0, 0, 0, 0);
                return;
            }

            HidePreviewUI();
        }

        /// <summary>
        /// すべてのプレビュー表示を完全にリセットする（確定ボタン押下時などに使用）
        /// </summary>
        public void ResetAllPreviews()
        {
            // キャッシュをクリア
            cachedActionDeltaHP = 0;
            cachedActionDeltaMP = 0;
            cachedActionDeltaCond = 0;
            cachedActionDeltaEval = 0;
            cachedActionDeltaLove = 0;
            cachedActionDeltaPublicEye = 0;
            cachedActionDeltaAbility = 0;

            HidePreviewUI();
        }

        // --- 内部メソッド ---

        /// <summary>
        /// プレビュー UI を非表示にする
        /// </summary>
        private void HidePreviewUI()
        {
            // 点滅処理を停止
            StopBlinking();

            // すべてのプレビュースライダーを非表示
            if (husbandHealthPreviewSlider != null)
            {
                husbandHealthPreviewSlider.gameObject.SetActive(false);
            }

            if (husbandMentalPreviewSlider != null)
            {
                husbandMentalPreviewSlider.gameObject.SetActive(false);
            }

            // すべてのプレビューテキストを非表示
            if (husbandHealthPreviewText != null)
            {
                husbandHealthPreviewText.gameObject.SetActive(false);
            }

            if (husbandMentalPreviewText != null)
            {
                husbandMentalPreviewText.gameObject.SetActive(false);
            }

            ResetArrowTexts();
            ResetSlidersToCurrentValues();
        }

        /// <summary>
        /// 矢印テキストをすべてリセット
        /// </summary>
        private void ResetArrowTexts()
        {
            if (conditionArrowText != null)
            {
                conditionArrowText.SetText("");
                conditionArrowText.color = Color.white;
            }

            if (evaluationArrowText != null)
            {
                evaluationArrowText.SetText("");
                evaluationArrowText.color = Color.white;
            }

            if (loveArrowText != null)
            {
                loveArrowText.SetText("");
                loveArrowText.color = Color.white;
            }

            if (publicEyeArrowText != null)
            {
                publicEyeArrowText.SetText("");
                publicEyeArrowText.color = Color.white;
            }
        }

        /// <summary>
        /// メインスライダーを現在値にリセット
        /// </summary>
        private void ResetSlidersToCurrentValues()
        {
            RuntimePlayerStatus currentStatus = GameSimulationManager.Instance.Husband;
            if (currentStatus != null)
            {
                if (husbandHealthSlider != null)
                {
                    husbandHealthSlider.value = currentStatus.CurrentHealth;
                }
                if (husbandMentalSlider != null)
                {
                    husbandMentalSlider.value = currentStatus.CurrentMental;
                }
            }
        }

        private void UpdateGlobalInfo(RuntimeDate date, RuntimeHouseholdBudget budget)
        {
            // 日付表示
            stringBuilder.Clear();
            stringBuilder.Append("結婚: ");
            stringBuilder.Append(date.Year).Append("年目 ");
            stringBuilder.Append(date.Month).Append("月");
            dateText.SetText(stringBuilder);

            // 資金表示
            stringBuilder.Clear();
            stringBuilder.Append("資金: ¥");
            stringBuilder.Append(budget.CurrentSavings.ToString("N0"));
            totalMoneyText.SetText(stringBuilder);

            // 固定費表示
            stringBuilder.Clear();
            stringBuilder.Append("固定費: ¥-");
            stringBuilder.Append(budget.FixedCost.TotalCost.ToString("N0"));
            stringBuilder.Append(" / 月");
            fixedCostText.SetText(stringBuilder);
            fixedCostText.color = Color.red;
        }

        private void UpdateEvaluationInfo(RuntimeReputation reputation)
        {
            // 夫婦仲表示
            stringBuilder.Clear();
            stringBuilder.Append("夫婦仲: ").Append(reputation.CurrentLove.ToString("F1")).Append(reputation.MAX_LOVE);
            loveText.SetText(stringBuilder);

            // チーム評価表示
            stringBuilder.Clear();
            stringBuilder.Append("チーム評価: ").Append(reputation.CurrentTeamEvaluation.ToString("F1"));
            teamEvaluationText.SetText(stringBuilder);

            // 世間の目表示
            stringBuilder.Clear();
            stringBuilder.Append(reputation.CurrentPublicEye.ToString("F1"));
            publicEyeText.SetText(stringBuilder);
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
            stringBuilder.Clear();
            stringBuilder.Append("体力: ").Append(husband.CurrentHealth).Append(" / ").Append(husband.MAX_HEALTH);
            husbandHealthText.SetText(stringBuilder);

            // マジックナンバー(100)を定数に置き換え
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

        private void UpdatePreviewSlider(Slider mainSlider, Slider previewSlider, TextMeshProUGUI previewText, int current, int max, int delta)
        {
            if (mainSlider == null || previewSlider == null || previewText == null)
            {
                return;
            }

            if (delta == 0)
            {
                // 変化がない場合はプレビューを非表示にし、メインスライダーをリセット
                previewSlider.gameObject.SetActive(false);
                previewText.gameObject.SetActive(false);
                mainSlider.value = current;
                return;
            }

            previewSlider.gameObject.SetActive(true);
            previewSlider.maxValue = max;

            int nextValue = Mathf.Clamp(current + delta, 0, max);
            bool isIncrease = delta > 0;

            if (isIncrease)
            {
                // 増加時: プレビュースライダーを次の値に設定
                previewSlider.value = nextValue;
                // メインスライダーは現在値のまま
                mainSlider.value = current;
            }
            else
            {
                // 減少時: メインスライダーを減少後の値に変更し、プレビュー（赤）が見えるようにする
                previewSlider.value = current;
                mainSlider.value = nextValue;
            }

            // キャッシュされたImage参照を使用、なければ取得してキャッシュ
            Image fillImage = null;
            if (previewSlider == husbandHealthPreviewSlider)
            {
                InitializeImageCacheIfNeeded(previewSlider, ref husbandHealthPreviewFillImage);
                fillImage = husbandHealthPreviewFillImage;
            }
            else if (previewSlider == husbandMentalPreviewSlider)
            {
                InitializeImageCacheIfNeeded(previewSlider, ref husbandMentalPreviewFillImage);
                fillImage = husbandMentalPreviewFillImage;
            }

            if (fillImage != null)
            {
                Color color = isIncrease ? increaseColor : decreaseColor;
                color.a = MAX_BLINK_ALPHA; // アルファ値は Update で制御するため初期値を最大に
                fillImage.color = color;
            }

            if (previewText != null)
            {
                previewText.gameObject.SetActive(true);
                string sign = isIncrease ? "+" : "-";
                stringBuilder.Clear();
                stringBuilder.Append(sign).Append(Mathf.Abs(delta));
                previewText.SetText(stringBuilder);
                previewText.color = isIncrease ? increaseColor : decreaseColor;
            }
        }

        /// <summary>
        /// 矢印テキストを更新
        /// </summary>
        /// <param name="arrowText"></param>
        /// <param name="delta"></param>
        private void UpdateArrowText(TextMeshProUGUI arrowText, int delta)
        {
            if (arrowText == null)
            {
                return;
            }

            if (delta > 0)
            {
                arrowText.SetText("↑ " + delta);
                arrowText.color = increaseColor;
            }
            else if (delta < 0)
            {
                arrowText.SetText("↓ " + Mathf.Abs(delta));
                arrowText.color = decreaseColor;
            }
            else
            {
                arrowText.SetText("-");
                arrowText.color = Color.white;
            }
        }
    }
}
