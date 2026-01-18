using Ambition.Data.Master;
using Ambition.Core.Managers;
using Ambition.Data.Runtime;
using Ambition.UI.MainGame.Parts;
using Cysharp.Threading.Tasks;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Ambition.UI.MainGame
{
    /// <summary>
    /// メイン画面のUI表示を管理するクラス
    /// </summary>
    public class MainGameView : MonoBehaviour
    {
        // --- 定数 ---

        /// <summary>
        /// 点滅周期
        /// </summary>
        private const float BLINK_CYCLE = 1.0f;

        /// <summary>
        /// 点滅時の最小アルファ値
        /// </summary>
        private const float MIN_BLINK_ALPHA = 0.5f;

        /// <summary>
        /// 点滅時の最大アルファ値
        /// </summary>
        private const float MAX_BLINK_ALPHA = 1.0f;

        // --- UIコンポーネントへの参照 ---

        [Header("Global Info")]
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private TextMeshProUGUI totalMoneyText;
        [SerializeField] private TextMeshProUGUI fixedCostText;

        [Header("Reputation UI")]
        [SerializeField] private TextMeshProUGUI loveText;
        [SerializeField] private TextMeshProUGUI teamEvaluationText;
        [SerializeField] private TextMeshProUGUI publicEyeText;

        [Header("Status Views")]
        [SerializeField] private HusbandStatusView husbandStatusView;
        [SerializeField] private WifeStatusView wifeStatusView;

        [Header("Command Buttons")]
        [SerializeField] private Button buttonCare;
        [SerializeField] private Button buttonSupport;
        [SerializeField] private Button buttonSNS;
        [SerializeField] private Button buttonDiscipline;
        [SerializeField] private Button buttonTalk;
        [SerializeField] private Button buttonRest;

        [Header("Confirm Button")]
        [SerializeField] private Button confirmButton;

        [Header("Sub Controllers")]
        [SerializeField] private SubMenuController subMenuController;
        [SerializeField] private ActionFlowController actionFlowController;

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

        // --- プレビュー点滅制御 ---
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
        /// アクションフローコントローラーへのアクセス
        /// </summary>
        public ActionFlowController ActionFlowController => actionFlowController;

        // --- MonoBehaviourコールバック ---

        private void OnDestroy()
        {
            StopBlinking();
        }

        // --- 公開メソッド ---

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
        public void RefreshView(RuntimeDate date, RuntimeHouseholdBudget budget, RuntimePlayerStatus husband, RuntimeWifeStatus wife, RuntimeReputation reputation)
        {
            UpdateGlobalInfo(date, budget);
            if (husbandStatusView != null)
            {
                husbandStatusView.UpdateHusbandInfo(husband);
            }
            if (wifeStatusView != null)
            {
                wifeStatusView.UpdateWifeInfo(wife);
            }
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
                cachedActionDeltaHP = 0;
                cachedActionDeltaMP = 0;
                cachedActionDeltaCond = 0;
                cachedActionDeltaEval = 0;
                cachedActionDeltaLove = 0;
                cachedActionDeltaPublicEye = 0;
                cachedActionDeltaAbility = 0;
                return;
            }

            cachedActionDeltaHP += action.DeltaHP;
            cachedActionDeltaMP += action.DeltaMP;
            cachedActionDeltaCond += action.DeltaCOND;
            cachedActionDeltaEval = action.DeltaTeamEvaluation;
            cachedActionDeltaLove = action.DeltaLove;
            cachedActionDeltaPublicEye = action.DeltaPublicEye;
            cachedActionDeltaAbility = 0;
        }

        public void UpdateSelectedMenu(FoodMitModel menu)
        {
            cachedActionDeltaHP += menu.MitigHP;
            cachedActionDeltaMP += menu.MitigMP;
            cachedActionDeltaCond += menu.MitigCOND;
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

            if (husbandStatusView != null)
            {
                UpdatePreviewSlider(
                    husbandStatusView.HealthSlider,
                    husbandHealthPreviewSlider,
                    husbandHealthPreviewText,
                    currentStatus.CurrentHealth,
                    currentStatus.MAX_HEALTH,
                    totalDeltaHP);

                UpdatePreviewSlider(
                    husbandStatusView.MentalSlider,
                    husbandMentalPreviewSlider,
                    husbandMentalPreviewText,
                    currentStatus.CurrentMental,
                    currentStatus.MAX_MENTAL,
                    totalDeltaMP);
            }

            UpdateArrowText(conditionArrowText, totalDeltaCond);
            UpdateArrowText(evaluationArrowText, totalDeltaEval);
            UpdateArrowText(loveArrowText, totalDeltaLove);
            UpdateArrowText(publicEyeArrowText, totalDeltaPublicEye);

            StartBlinkingAsync().Forget();
        }

        /// <summary>
        /// プレビュー表示を非表示にする
        /// </summary>
        public void HidePreview()
        {
            if (cachedActionDeltaHP != 0 || cachedActionDeltaMP != 0 || cachedActionDeltaCond != 0 ||
                cachedActionDeltaEval != 0 || cachedActionDeltaLove != 0 || cachedActionDeltaPublicEye != 0)
            {
                ShowPreview(0, 0, 0, 0, 0, 0);
                return;
            }

            if (husbandHealthPreviewSlider != null)
            {
                husbandHealthPreviewSlider.gameObject.SetActive(false);
            }

            if (husbandMentalPreviewSlider != null)
            {
                husbandMentalPreviewSlider.gameObject.SetActive(false);
            }

            if (husbandHealthPreviewText != null)
            {
                husbandHealthPreviewText.gameObject.SetActive(false);
            }

            if (husbandMentalPreviewText != null)
            {
                husbandMentalPreviewText.gameObject.SetActive(false);
            }

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

            RuntimePlayerStatus currentStatus = GameSimulationManager.Instance.Husband;
            if (currentStatus != null && husbandStatusView != null)
            {
                if (husbandStatusView.HealthSlider != null)
                {
                    husbandStatusView.HealthSlider.value = currentStatus.CurrentHealth;
                }

                if (husbandStatusView.MentalSlider != null)
                {
                    husbandStatusView.MentalSlider.value = currentStatus.CurrentMental;
                }
            }
        }

        /// <summary>
        /// すべてのプレビュー表示を完全にリセット
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

            // すべての矢印テキストをクリア
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

            // メインスライダーを現在値にリセット
            RuntimePlayerStatus currentStatus = GameSimulationManager.Instance.Husband;
            if (currentStatus != null && husbandStatusView != null)
            {
                if (husbandStatusView.HealthSlider != null)
                {
                    husbandStatusView.HealthSlider.value = currentStatus.CurrentHealth;
                }
                if (husbandStatusView.MentalSlider != null)
                {
                    husbandStatusView.MentalSlider.value = currentStatus.CurrentMental;
                }
            }
        }

        // --- 内部メソッド ---

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

        private void UpdatePreviewSlider(Slider mainSlider, Slider previewSlider, TextMeshProUGUI previewText, int current, int max, int delta)
        {
            if (mainSlider == null || previewSlider == null || previewText == null)
            {
                return;
            }

            if (delta == 0)
            {
                // 変化がない場合はプレビューを非表示にする
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
                previewSlider.value = nextValue;
                mainSlider.value = current;
            }
            else
            {
                // 減少時: メインスライダーを減少後の値に変更し、プレビュー（赤）が見えるようにする
                previewSlider.value = current;
                mainSlider.value = nextValue;
            }

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
                color.a = MAX_BLINK_ALPHA;
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

        private async UniTaskVoid StartBlinkingAsync()
        {
            StopBlinking();

            blinkCancellationTokenSource = new CancellationTokenSource();
            var token = blinkCancellationTokenSource.Token;

            float elapsedTime = 0f;
            while (token.IsCancellationRequested == false)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(MIN_BLINK_ALPHA, MAX_BLINK_ALPHA, Mathf.Sin((elapsedTime * Mathf.PI * 2 / BLINK_CYCLE) + 1) * 0.5f);

                if (husbandHealthPreviewSlider != null && husbandHealthPreviewSlider.gameObject.activeSelf)
                {
                    InitializeImageCacheIfNeeded(husbandHealthPreviewSlider, ref husbandHealthPreviewFillImage);
                    if (husbandHealthPreviewFillImage != null)
                    {
                        var color = husbandHealthPreviewFillImage.color;
                        color.a = alpha;
                        husbandHealthPreviewFillImage.color = color;
                    }
                }

                if (husbandMentalPreviewSlider != null && husbandMentalPreviewSlider.gameObject.activeSelf)
                {
                    InitializeImageCacheIfNeeded(husbandMentalPreviewSlider, ref husbandMentalPreviewFillImage);
                    if (husbandMentalPreviewFillImage != null)
                    {
                        var color = husbandMentalPreviewFillImage.color;
                        color.a = alpha;
                        husbandMentalPreviewFillImage.color = color;
                    }
                }

                if (husbandHealthPreviewText != null && husbandHealthPreviewText.gameObject.activeSelf)
                {
                    var color = husbandHealthPreviewText.color;
                    color.a = alpha;
                    husbandHealthPreviewText.color = color;
                }

                if (husbandMentalPreviewText != null && husbandMentalPreviewText.gameObject.activeSelf)
                {
                    var color = husbandMentalPreviewText.color;
                    color.a = alpha;
                    husbandMentalPreviewText.alpha = alpha;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

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

        private void InitializeImageCacheIfNeeded(Slider slider, ref Image cachedImage)
        {
            if (cachedImage == null && slider != null && slider.fillRect != null)
            {
                cachedImage = slider.fillRect.GetComponent<Image>();
            }
        }
    }
}
