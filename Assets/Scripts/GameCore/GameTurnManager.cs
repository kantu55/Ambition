using Ambition.Core.Managers;
using Ambition.Data.Master;
using Ambition.UI.Panels;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        public async UniTask ExecuteTurnAsync(WifeActionModel action, FoodMitModel food)
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

            // Pick random event
            EventModel randomEvent = PickRandomEvent();

            if (randomEvent != null && eventDialogPanel != null)
            {
                Debug.Log($"[GameTurnManager] Showing event: {randomEvent.Title}");

                // Show event dialog and wait for user to confirm
                var tcs = new UniTaskCompletionSource();
                currentEventTask = tcs;

                eventDialogPanel.ShowEvent(randomEvent);

                await tcs.Task;
                currentEventTask = null;
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// Phase 4: パラメータ変動の表示
        /// </summary>
        private async UniTask ShowParameterChangePhaseAsync(WifeActionModel action, FoodMitModel food)
        {
            Debug.Log("[GameTurnManager] Phase 4: Parameter changes");

            // キャリア
            // phase_id = f(career_month, rookie_graduation, veteran_threshold)
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
            float veteranExtraDecayAB = 0f;
            if (careerType == CareerType.VETERAN)
            {
                veteranExtraDecayHP = veteranExtraDecayDatas.FirstOrDefault(v => (StatType)v.StatType == StatType.HP)?.Value ?? 0f;
                veteranExtraDecayMP = veteranExtraDecayDatas.FirstOrDefault(v => (StatType)v.StatType == StatType.MP)?.Value ?? 0f;
                veteranExtraDecayCOND = veteranExtraDecayDatas.FirstOrDefault(v => (StatType)v.StatType == StatType.COND)?.Value ?? 0f;
                veteranExtraDecayAB = veteranExtraDecayDatas.FirstOrDefault(v => (StatType)v.StatType == StatType.AB)?.Value ?? 0f;
            }

            var baseDeltaForFoodHP = (int)(phaseBaseDecayHP + matchExtraDecayHP + veteranExtraDecayHP);
            var baseDeltaForFoodMP = (int)(phaseBaseDecayMP + matchExtraDecayMP + veteranExtraDecayMP);
            var baseDeltaForFoodCOND = (int)(phaseBaseDecayCOND + matchExtraDecayCOND + veteranExtraDecayCOND);
            var baseDeltaForFoodAB = veteranExtraDecayAB;
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

            // param_next[x] = param_prev[x] + food_delta_applied[x] + action_delta[x] + event_param_delta[x] + match_param_delta[x] + abnormal_delta[x]...
            // 注意点：
            // food_delta_applied があるのは HP / MP / CONDのみ
            // T, RL, RP, CP などは 食事で相殺されない（行動 / イベント / 試合でのみ動く）
            var paramNextHP = foogDeltaAppliedHP + action.DeltaHP;
            var paramNextMP = foogDeltaAppliedMP + action.DeltaMP;
            var paramNextCOND = foogDeltaAppliedCOND + action.DeltaCOND;

            if (parameterChangePanel != null && GameSimulationManager.Instance != null)
            {
                Dictionary<string, int> changes = new Dictionary<string, int>();
                changes["体力"] = paramNextHP;
                changes["メンタル"] = paramNextMP;
                changes["コンディション"] = paramNextCOND;
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

        }

        private EventModel PickRandomEvent()
        {
            var eventDataList = DataManager.Instance.GetDatas<EventModel>();
            if (eventDataList == null || eventDataList.Any() == false)
            {
                return null;
            }

            var eventData = eventDataList[UnityEngine.Random.Range(0, eventDataList.Count)];
            Debug.Log($"ランダムイベント発生: {eventData.Title} (ID:{eventData.EventId})");
            return eventData;
        }


        private void OnMatchResultContinue()
        {
            currentMatchTask?.TrySetResult();
        }

        private void OnEventConfirmed(EventModel eventData)
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
