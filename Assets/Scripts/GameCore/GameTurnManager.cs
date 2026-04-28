using Ambition.Core.Managers;
using Ambition.Data.Master;
using Ambition.Data.Master.Event;
using Ambition.UI.Panels;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ambition.GameCore
{
    /// <summary>
    /// ゲームのターン管理を行うクラス
    /// </summary>
    public class GameTurnManager : MonoBehaviour
    {
        public enum MatchResultTier
        {
            GREAT = 1,
            GOOD,
            NORMAL,
            BAD,
            TERRIBLE
        }

        public enum MatchDifficulty
        {
            EASY = 1,
            NORMAL,
            HARD,
        }

        public enum CareerType
        {
            ROOKIE = 1,
            MID,
            VETERAN,
        }

        public enum StatType
        {
            HP = 1,
            MP,
            COND,
            AB,
        }

        public enum FoodTier
        {
            POVERTY = 1,
            Q0,
            Q1,
            Q2,
            Q3,
            Q4,
        }

        public enum CondStage
        {
            STAGE0 = 1,
            STAGE1,
            STAGE2,
            STAGE3,
            STAGE4,
        }

        [SerializeField] private MatchResultPanel matchPanel;
        [SerializeField] private EventDialogPanel eventDialogPanel;
        [SerializeField] private ParameterChangePanel parameterChangePanel;
        [SerializeField] private MonthlyReportPanel monthlyReportPanel;

        /// <summary>
        /// ターン完了時に発生するイベント
        /// </summary>
        public event Action OnTurnCompleted;

        /// <summary>
        /// ターン進行開始時に発生するイベント
        /// </summary>
        public event Action OnTurnFlowStarted;

        /// <summary>
        /// ターン進行中かどうか
        /// </summary>
        private bool isProcessingTurn = false;

        private UniTaskCompletionSource currentEventTask;
        private UniTaskCompletionSource currentMatchTask;
        private UniTaskCompletionSource currentParamTask;
        private UniTaskCompletionSource currentReportTask;

        private int pendingEventMitigHP;
        private int pendingEventMitigMP;
        private int pendingEventMitigCOND;
        private int pendingEventMitigCost;

        private void Awake()
        {
            if (matchPanel != null)
            {
                matchPanel.OnContinueClicked += OnMatchResultContinue;
            }

            if (eventDialogPanel != null)
            {
                eventDialogPanel.OnEventConfirmed += OnEventConfirmed;
                eventDialogPanel.OnEventCancelled += OnEventCancelled;
            }

            if (parameterChangePanel != null)
            {
                parameterChangePanel.OnContinueClicked += OnParameterChangeContinue;
            }

            if (monthlyReportPanel != null)
            {
                monthlyReportPanel.OnContinueClicked += OnMonthlyReportContinue;
            }
        }

        /// <summary>
        /// ターン進行を一元管理する非同期メソッド
        /// </summary>
        public async UniTask ExecuteTurnAsync(WifeActionModel action, FoodModel food)
        {
            if (isProcessingTurn)
            {
                Debug.LogWarning("[GameTurnManager] Already processing a turn");
                return;
            }

            isProcessingTurn = true;
            OnTurnFlowStarted?.Invoke();

            Debug.Log($"[GameTurnManager] Starting turn execution: {action.Name}");

            // Phase 1: Process action and apply effects
            ProcessActionPhase(action);

            // Phase 2: Show match results (if applicable)
            await ShowMatchResultPhaseAsync();

            // Phase 3: Show random event (if any)
            await ShowEventPhaseAsync();

            // Phase 4: Show parameter changes
            await ShowParameterChangePhaseAsync(action, food);

            // Phase 5: Show monthly report
            await ShowMonthlyReportPhaseAsync();

            // Phase 6: Finalize turn (increment turn counter, etc.)
            FinalizeTurn();

            Debug.Log("[GameTurnManager] Turn execution completed");
            OnTurnCompleted?.Invoke();
            isProcessingTurn = false;
        }

        private void ProcessActionPhase(WifeActionModel action)
        {
            Debug.Log("[GameTurnManager] Phase 1: Processing action");

            if (GameSimulationManager.Instance == null)
            {
                Debug.LogError("[GameTurnManager] GameSimulationManager.Instance is null");
                return;
            }

            // Check requirements (money, stamina, etc.)
            if (GameSimulationManager.Instance.Budget.CurrentSavings < action.CashCost)
            {
                Debug.LogWarning("[GameTurnManager] Insufficient funds for action");
                return;
            }

            // Consume resources (money)
            if (action.CashCost > 0)
            {
                GameSimulationManager.Instance.Budget.TrySpend(action.CashCost);
            }
            else if (action.CashCost < 0)
            {
                GameSimulationManager.Instance.Budget.AddIncome(Mathf.Abs(action.CashCost));
            }

            // Apply effects to husband
            if (action.DeltaHP != 0)
            {
                GameSimulationManager.Instance.Husband.ChangeHealth(action.DeltaHP);
            }

            if (action.DeltaMP != 0)
            {
                GameSimulationManager.Instance.Husband.ChangeMental(action.DeltaMP);
            }
        }

        /// <summary>
        /// Phase 2: 試合結果の表示
        /// </summary>
        private async UniTask ShowMatchResultPhaseAsync()
        {
            // effective_AB = AB + cond_coef * (COND - 50) + mp_coef * (MP - 50)
            var difficulty = MatchDifficulty.NORMAL; // 仮にNORMAL固定
            var difficultyDatas = DataManager.Instance.GetDatas<MatchDifficultyModel>();
            if (difficultyDatas == null || difficultyDatas.Count == 0)
            {
                Debug.LogError("[GameTurnManager] MatchDifficultyModel data not found");
                return;
            }

            var difficultyData = difficultyDatas.FirstOrDefault(v => (MatchDifficulty)v.DifficultyType == difficulty);
            var condCoef = difficultyData != null ? difficultyData.ConditionCoefficient : 0.2f;
            var mpCoef = difficultyData != null ? difficultyData.MpCoefficient : 0.15f;
            var husband = GameSimulationManager.Instance.Husband;
            var cond = condCoef * (husband.CurrentCondition - 50);
            var mp = mpCoef * (husband.CurrentMental - 50);
            var effectiveAB = husband.CurrentAbility + condCoef * (husband.CurrentCondition - 50) + mpCoef * (husband.CurrentMental - 50);
            Debug.Log($"[MatchDifficulty:{difficulty}] effective_AB[{effectiveAB}] = AB[{husband.CurrentAbility}] + COND[{cond}] + MP[{mp}]\n" +
                $"AB[{husband.CurrentAbility}] = CurrentAB[{husband.CurrentAbility}]\n" +
                $"COND[{cond}] = cond_coef[{condCoef}] * (CurrentCOND[{husband.CurrentCondition}] - 50)\n" +
                $"MP[{mp}] = mp_coef[{mpCoef}] * (CurrentMP[{husband.CurrentMental}] - 50)");

            // rating = effective_AB - baseline_ab(year)
            var baselineDatas = DataManager.Instance.GetDatas<BaselineAbByYearModel>();
            if (baselineDatas == null || baselineDatas.Count == 0)
            {
                Debug.LogError("[GameTurnManager] BaselineAbByYearModel data not found");
                return;
            }

            var year = GameSimulationManager.Instance.Date.Year;
            var baselineData = baselineDatas.FirstOrDefault(v => v.Year == year);
            var baselineAB = baselineData != null ? baselineData.BaselineAb : 9;
            var rating = effectiveAB - baselineAB;
            Debug.Log($"[Year:{year}] rating[{rating}] = effective_AB[{effectiveAB}] - baseline_ab[{baselineAB}]");

            // Tier判定
            //rating を performance_rating_thresholds の閾値で GREAT/ GOOD / NORMAL / BAD / TERRIBLE に分類
            MatchResultTier tier = MatchResultTier.TERRIBLE;
            var performanceThresholds = DataManager.Instance.GetDatas<PerformanceRatingThresholdsModel>();
            if (performanceThresholds == null || performanceThresholds.Count == 0)
            {
                Debug.LogError("[GameTurnManager] PerformanceRatingThresholdsModel data not found");
                return;
            }

            foreach (var performanceThreshold in performanceThresholds)
            {
                if (rating > performanceThreshold.RatingAtLeast)
                {
                    tier = (MatchResultTier)performanceThreshold.Tier;
                    break;
                }
            }

            // 試合結果
            Debug.Log($"[GameTurnManager] Phase 2: Match results {tier}");

            var tcs = new UniTaskCompletionSource();
            currentMatchTask = tcs;

            matchPanel.ShowMatchResult("試合結果", tier.ToString());

            await tcs.Task;
            currentMatchTask = null;

            // チーム評価 / 契約用の評価ポイント 変動の適用
            var matchTierDelta = DataManager.Instance.GetDatas<MatchTierDeltaModel>();
            if (matchTierDelta == null || matchTierDelta.Count == 0)
            {
                Debug.LogError("[GameTurnManager] MatchTierDeltaModel data not found");
                return;
            }

            var tierDelta = matchTierDelta.FirstOrDefault(v => (MatchResultTier)v.Tier == tier);
            var deltaT = tierDelta != null ? tierDelta.DeltaT : 0;
            var deltaCP = tierDelta != null ? tierDelta.DeltaCp : 0;
            GameSimulationManager.Instance.Reputation.ChangeTeamEvaluation(deltaT);
            GameSimulationManager.Instance.Reputation.ChangeCP(deltaCP);

            //追加で怪我判定が走り、怪我なら「怪我状態」へ（反映は月末側）
            // ・試合月：怪我判定あり。	
            //・オフ月：原則は怪我なし。ただし練習 / 投資内容によって低確率で判定し得る。	
            //	
            //【怪我率（調整対象）】	
            //・怪我率は主にHP帯で決める（例：HP >= 60 / 45 - 59 / 30 - 44 / 10 - 29 /<= 9 など）。	
            //・CONDは怪我率に関与する（試合マスタの怪我率加算に含める）。	
            //	
            //【重症度（調整対象）】	
            //・HPが低いほど重症 / 重篤を引きやすい。	
            //・重篤：即引退（career_end = true）。	
            //	
            //【怪我結果】	
            //・軽 / 中 / 重：治療ターン 1 / 2 / 3。怪我中は練習不可。治療で資産消費。	
            //・入院：1ヶ月欠場ペナルティ。1ヶ月で全回復（確定）。	
            //・重篤：即引退（career_end = true）。	
            //	
            //※引退判定は「重篤（career_end）」でのみ即死。HP <= 0で即ENDにはしない。	

            await UniTask.Yield();
        }

        /// <summary>
        /// Phase 3: イベントの表示
        /// </summary>
        private async UniTask ShowEventPhaseAsync()
        {
            Debug.Log("[GameTurnManager] Phase 3: Random event");

            if (GameSimulationManager.Instance == null)
            {
                return;
            }

            int currentMonth = GameSimulationManager.Instance.Date.Month;
            var schedule = GameSimulationManager.Instance.EventSchedule;
            int scheduledEventId = schedule != null ? schedule.GetEventIdForMonth(currentMonth) : -1;

            if (scheduledEventId != -1)
            {
                var eventData = DataManager.Instance.GetDatas<EventMaster>().FirstOrDefault(e => e.EventId == scheduledEventId);
                if (eventData != null)
                {
                    var selectedOptions = await eventDialogPanel.ShowEventAsync(eventData);
                    ApplyEventOptionEffects(selectedOptions);
                }
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// Phase 4: パラメータ変動の表示
        /// </summary>
        private async UniTask ShowParameterChangePhaseAsync(WifeActionModel action, FoodModel food)
        {
            Debug.Log("[GameTurnManager] Phase 4: Parameter changes");

            // キャリア
            // phase_id = f(career_month, rookie_graduation, veteran_threshold)
            // TODO:正しいキャリア判定ロジックに置き換え
            int year = GameSimulationManager.Instance.Date.Year;
            CareerType careerType = CareerType.ROOKIE;
            if (year > 12)
            {
                careerType = CareerType.VETERAN;

            }
            else if (year > 1)
            {
                careerType = CareerType.MID;
            }



            // 3 - A) 基礎減少（ドリフト）を作る（HP / MP / COND）
            // phase_base_delta（新人 / 通常 / 真エンドなどで差し替わり）
            // match_extra_delta（試合月のみ加算）
            // veteran_extra_delta（ベテランなら加算）
            // 基礎減少の素（食事で軽減される対象）
            // base_delta_for_food[s] = phase_base_delta[s] + match_extra_delta[s] + veteran_extra_delta[s]
            // s ∈ { HP, MP, COND}
            // ※POVERTYのみ、さらにペナルティを「基礎減少に加算（悪化）」

            var phaseBaseDecayDatas = DataManager.Instance.GetDatas<BaseDecayPerMonthModel>();
            if (phaseBaseDecayDatas == null || phaseBaseDecayDatas.Count == 0)
            {
                Debug.LogError("[GameTurnManager] BaseDecayPerMonthModel data not found");
                return;
            }

            var matchExtraDecayDatas = DataManager.Instance.GetDatas<MatchMonthExtraDecayModel>();
            if (matchExtraDecayDatas == null || matchExtraDecayDatas.Count == 0)
            {
                Debug.LogError("[GameTurnManager] MatchMonthExtraDecayModel data not found");
                return;
            }


            var veteranExtraDecayDatas = DataManager.Instance.GetDatas<VeteranExtraDecayModel>();
            if (veteranExtraDecayDatas == null || veteranExtraDecayDatas.Count == 0)
            {
                Debug.LogError("[GameTurnManager] VeteranExtraDecayModel data not found");
                return;
            }

            float phaseBaseDecayHP = phaseBaseDecayDatas.FirstOrDefault(v => (CareerType)v.PlayerCareerStage == careerType && (StatType)v.StatType == StatType.HP)?.Value ?? -1f;
            float phaseBaseDecayMP = phaseBaseDecayDatas.FirstOrDefault(v => (CareerType)v.PlayerCareerStage == careerType && (StatType)v.StatType == StatType.MP)?.Value ?? -1f;
            float phaseBaseDecayCOND = phaseBaseDecayDatas.FirstOrDefault(v => (CareerType)v.PlayerCareerStage == careerType && (StatType)v.StatType == StatType.COND)?.Value ?? -1f;

            float matchExtraDecayHP = matchExtraDecayDatas.FirstOrDefault(v => (CareerType)v.PlayerCareerStage == careerType && (StatType)v.StatType == StatType.HP)?.Value ?? 1f;
            float matchExtraDecayMP = matchExtraDecayDatas.FirstOrDefault(v => (CareerType)v.PlayerCareerStage == careerType && (StatType)v.StatType == StatType.MP)?.Value ?? 1f;
            float matchExtraDecayCOND = matchExtraDecayDatas.FirstOrDefault(v => (CareerType)v.PlayerCareerStage == careerType && (StatType)v.StatType == StatType.COND)?.Value ?? 1f;

            float veteranExtraDecayHP = 0f;
            float veteranExtraDecayMP = 0f;
            float veteranExtraDecayCOND = 0f;
            if (careerType == CareerType.VETERAN)
            {
                veteranExtraDecayHP = veteranExtraDecayDatas.FirstOrDefault(v => (StatType)v.StatType == StatType.HP)?.Value ?? 0f;
                veteranExtraDecayMP = veteranExtraDecayDatas.FirstOrDefault(v => (StatType)v.StatType == StatType.MP)?.Value ?? 0f;
                veteranExtraDecayCOND = veteranExtraDecayDatas.FirstOrDefault(v => (StatType)v.StatType == StatType.COND)?.Value ?? 0f;
            }

            var baseDeltaForFoodHP = (int)(phaseBaseDecayHP + matchExtraDecayHP + veteranExtraDecayHP);
            var baseDeltaForFoodMP = (int)(phaseBaseDecayMP + matchExtraDecayMP + veteranExtraDecayMP);
            var baseDeltaForFoodCOND = (int)(phaseBaseDecayCOND + matchExtraDecayCOND + veteranExtraDecayCOND);
            Debug.Log($"Base Delta for Food - HP: {baseDeltaForFoodHP}, MP: {baseDeltaForFoodMP}, COND: {baseDeltaForFoodCOND}");

            // 3 - B) 食事軽減（mitig）を算出（HP / MP / COND）
            // mitig_total = plan.mitig_total
            // mitig_dist = menu の配分（HP / MP / COND合計 = mitig_total）
            // mitig[s] = mitig_dist[s]（必要ならCookingLvの主軸ボーナスを加算）
            var mitigHP = food.MitigHP;
            var mitigMP = food.MitigMP;
            var mitigCOND = food.MitigCOND;
            Debug.Log($"Mitigation - HP: {mitigHP}, MP: {mitigMP}, COND: {mitigCOND}");

            // 3 - C) 食事の適用（重要：食事は回復しない）
            // 食事適用後のドリフト
            // food_delta_applied[s] = min(0, base_delta_for_food[s] + mitig[s])
            // 上限が 0 なので、食事だけでプラス回復（HP + など）にはならない
            var foogDeltaAppliedHP = Math.Min(0, baseDeltaForFoodHP + mitigHP);
            var foogDeltaAppliedMP = Math.Min(0, baseDeltaForFoodMP + mitigMP);
            var foogDeltaAppliedCOND = Math.Min(0, baseDeltaForFoodCOND + mitigCOND);
            Debug.Log($"Food Delta Applied - HP: {foogDeltaAppliedHP}, MP: {foogDeltaAppliedMP}, COND: {foogDeltaAppliedCOND}");

            // 5) AB成長（別枠。月末で確定）
            // ABは「行動による成長補正」「食事倍率」「COND段階倍率」が乗ります。
            // base = ab_base_growth_per_month[phase]
            // raw = base + AB_growth_from_action（＝行動の growth_add）
            // mult = ab_growth_mult_by_food_plan[food_plan] * ab_growth_mult_by_cond_stage[cond_stage]
            // pos = min(ab_growth_cap_per_month.cap, raw * mult)
            // AB_next = clamp(0..100, AB + pos + (is_veteran ? veteran_extra_decay.AB : 0))
            var abBaseGrowthDatas = DataManager.Instance.GetDatas<AbBaseGrowthPerMonthModel>();
            if (abBaseGrowthDatas == null || abBaseGrowthDatas.Count == 0)
            {
                Debug.LogError("[GameTurnManager] AbBaseGrowthPerMonthModel data not found");
                return;
            }

            var foodGrowthMultDatas = DataManager.Instance.GetDatas<AbGrowthMultByFoodPlanModel>();
            if (foodGrowthMultDatas == null || foodGrowthMultDatas.Count == 0)
            {
                Debug.LogError("[GameTurnManager] AbGrowthMultByFoodPlanModel data not found");
                return;
            }

            var condGrowthMultDatas = DataManager.Instance.GetDatas<AbGrowthMultByCondStageModel>();
            if (condGrowthMultDatas == null || condGrowthMultDatas.Count == 0)
            {
                Debug.LogError("[GameTurnManager] AbGrowthMultByCondStageModel data not found");
                return;
            }

            float abBase = abBaseGrowthDatas.FirstOrDefault(v => (CareerType)v.PlayerCareerStage == careerType)?.Value ?? 1f;
            float foodMult = foodGrowthMultDatas.FirstOrDefault(v => (FoodTier)v.FoodTier == FoodTier.Q1)?.GrowthMulti ?? 1f;
            float condMult = condGrowthMultDatas.FirstOrDefault(v => (CondStage)v.CondStage == CondStage.STAGE2)?.GrowthMulti ?? 1f;
            float cap = DataManager.Instance.GetDatas<AbGrowthCapPerMonth>().FirstOrDefault()?.Cap ?? 10f;
            float veteranExtraDecayAB = 0f;
            if (careerType == CareerType.VETERAN)
            {
                veteranExtraDecayAB = veteranExtraDecayDatas.FirstOrDefault(v => (StatType)v.StatType == StatType.AB)?.Value ?? 0f;
            }

            float rawABGrowth = abBase + action.GrowthAdd;
            float mult = foodMult * condMult;
            float pos = Math.Min(cap, rawABGrowth * mult);
            float AbNext = Mathf.Clamp((float)GameSimulationManager.Instance.Husband.CurrentAbility + pos + veteranExtraDecayAB, 0, 100);
            Debug.Log($"AB Growth Calculation: base[{abBase}] + action_growth[{action.GrowthAdd}] = raw[{rawABGrowth}] * mult[{mult}] = pos[{pos}] + veteran_extra[{veteranExtraDecayAB}] = AB_next[{AbNext}]");


            // param_next[x] = param_prev[x] + food_delta_applied[x] + action_delta[x] + event_param_delta[x] + match_param_delta[x] + abnormal_delta[x]...
            // 注意点：
            // food_delta_applied があるのは HP / MP / CONDのみ
            // T, RL, RP, CP などは 食事で相殺されない（行動 / イベント / 試合でのみ動く）
            var paramNextHP = foogDeltaAppliedHP + action.DeltaHP + pendingEventMitigHP;
            var paramNextMP = foogDeltaAppliedMP + action.DeltaMP + pendingEventMitigMP;
            var paramNextCOND = foogDeltaAppliedCOND + action.DeltaCOND + pendingEventMitigCOND;

            pendingEventMitigHP = 0;
            pendingEventMitigMP = 0;
            pendingEventMitigCOND = 0;

            var husband = GameSimulationManager.Instance.Husband;
            husband.ChangeHealth(paramNextHP);
            husband.ChangeMental(paramNextMP);
            husband.ChangeCondition(paramNextCOND);
            husband.GrowAbility(AbNext);

            if (parameterChangePanel != null && GameSimulationManager.Instance != null)
            {
                Dictionary<string, double> changes = new Dictionary<string, double>();
                changes["体力"] = paramNextHP;
                changes["メンタル"] = paramNextMP;
                changes["コンディション"] = paramNextCOND;
                changes["能力値"] = AbNext;

                if (changes.Count > 0)
                {
                    var tcs = new UniTaskCompletionSource();
                    currentParamTask = tcs;
                    parameterChangePanel.ShowChanges("パラメータ変動", changes);
                    await tcs.Task;
                    currentParamTask = null;
                }
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// Phase 5: 月次報告の表示
        /// </summary>
        private async UniTask ShowMonthlyReportPhaseAsync()
        {
            Debug.Log("[GameTurnManager] Phase 5: Monthly report");

            if (monthlyReportPanel != null && GameSimulationManager.Instance != null)
            {
                var budget = GameSimulationManager.Instance.Budget;
                var date = GameSimulationManager.Instance.Date;
                var husband = GameSimulationManager.Instance.Husband;
                var wife = GameSimulationManager.Instance.Wife;

                string dateStr = $"{date.Year}年 {date.Month}月";
                int income = 0; // Calculate from budget history
                int expenses = (int)budget.FixedCost.TotalCost;
                int savings = (int)budget.CurrentSavings;

                string playerStats = $"体力: {husband.CurrentHealth}/{husband.MAX_HEALTH}\n" +
                                    $"メンタル: {husband.CurrentMental}/{husband.MAX_MENTAL}";

                string wifeStats = $"ケアレベル: {wife.CareLevel}\n" +
                                  $"PRレベル: {wife.PRLevel}\n" +
                                  $"コーチレベル: {wife.CoachLevel}";

                var tcs = new UniTaskCompletionSource();
                currentReportTask = tcs;

                monthlyReportPanel.ShowReport(dateStr, income, expenses, savings, playerStats, wifeStats);

                await tcs.Task;
                currentReportTask = null;
            }

            await UniTask.Yield();
        }

        private void FinalizeTurn()
        {
            if (GameSimulationManager.Instance == null)
            {
                return;
            }

            GameSimulationManager.Instance.ProceedTurn();
        }

        private void ApplyEventOptionEffects(List<EventOption> selectedOptions)
        {
            pendingEventMitigHP = 0;
            pendingEventMitigMP = 0;
            pendingEventMitigCOND = 0;
            pendingEventMitigCost = 0;

            if (selectedOptions == null)
            {
                return;
            }

            foreach (var option in selectedOptions)
            {
                if (option.CostMoney > 0)
                {
                    if (GameSimulationManager.Instance.Budget.TrySpend(option.CostMoney) == false)
                    {
                        Debug.LogWarning($"[GameTurnManager] Failed to spend money for event option {option.Id}");

                    }
                }
                else if (option.CostMoney < 0)
                {
                    GameSimulationManager.Instance.Budget.AddIncome(Math.Abs(option.CostMoney));
                }

                pendingEventMitigHP += option.MitigHp;
                pendingEventMitigMP += option.MitigMp;
                pendingEventMitigCOND += option.MitigCond;
            }

            Debug.Log($"[GameTurnManager] Event option effects applied - CostMoney processed, MitigHP: {pendingEventMitigHP}, MitigMP: {pendingEventMitigMP}, MitigCOND: {pendingEventMitigCOND}");
        }

        private void OnMatchResultContinue()
        {
            currentMatchTask?.TrySetResult();
        }

        private void OnEventConfirmed(EventMaster eventData)
        {
            if (eventData == null || GameSimulationManager.Instance == null)
            {
                return;
            }

            // todo: イベントデータに基づく処理を実装

            currentEventTask?.TrySetResult();
        }

        private void OnEventCancelled()
        {
            currentEventTask?.TrySetResult();
        }

        private void OnParameterChangeContinue()
        {
            currentParamTask?.TrySetResult();
        }

        private void OnMonthlyReportContinue()
        {
            currentReportTask?.TrySetResult();
        }
    }
}
