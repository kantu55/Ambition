using Ambition.Core.Managers;
using Ambition.Data.Master;
using Ambition.Data.Master.Event;
using Ambition.UI.Panels;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.Rendering.DebugUI.Table;

namespace Ambition.GameCore
{
    /// <summary>
    /// �Q�[���̃^�[���Ǘ����s���N���X
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
        /// �^�[���������ɔ�������C�x���g
        /// </summary>
        public event Action OnTurnCompleted;

        /// <summary>
        /// �^�[���i�s�J�n���ɔ�������C�x���g
        /// </summary>
        public event Action OnTurnFlowStarted;

        /// <summary>
        /// �^�[���i�s�����ǂ���
        /// </summary>
        private bool isProcessingTurn = false;

        private UniTaskCompletionSource currentEventTask;
        private UniTaskCompletionSource currentMatchTask;
        private UniTaskCompletionSource currentParamTask;
        private UniTaskCompletionSource currentReportTask;

        private int _pendingEventMitigHP;
        private int _pendingEventMitigMP;
        private int _pendingEventMitigCOND;

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
        /// �^�[���i�s���ꌳ�Ǘ�����񓯊����\�b�h
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
        /// Phase 2: �������ʂ̕\��
        /// </summary>
        private async UniTask ShowMatchResultPhaseAsync()
        {
            // effective_AB = AB + cond_coef * (COND - 50) + mp_coef * (MP - 50)
            var difficulty = MatchDifficulty.NORMAL; // ����NORMAL�Œ�
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

            // Tier����
            //rating �� performance_rating_thresholds ��臒l�� GREAT/ GOOD / NORMAL / BAD / TERRIBLE �ɕ���
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

            // ��������
            Debug.Log($"[GameTurnManager] Phase 2: Match results {tier}");

            var tcs = new UniTaskCompletionSource();
            currentMatchTask = tcs;

            matchPanel.ShowMatchResult("��������", tier.ToString());

            await tcs.Task;
            currentMatchTask = null;

            // �`�[���]�� / �_��p�̕]���|�C���g �ϓ��̓K�p
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

            //�ǉ��ŉ��䔻�肪����A����Ȃ�u�����ԁv�ցi���f�͌������j

            await UniTask.Yield();
        }

        /// <summary>
        /// Phase 3: �C�x���g�̕\��
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
        /// Phase 4: �p�����[�^�ϓ��̕\��
        /// </summary>
        private async UniTask ShowParameterChangePhaseAsync(WifeActionModel action, FoodModel food)
        {
            Debug.Log("[GameTurnManager] Phase 4: Parameter changes");

            // �L�����A
            // phase_id = f(career_month, rookie_graduation, veteran_threshold)
            // TODO:�������L�����A���胍�W�b�N�ɒu������
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



            // 3 - A) ��b�����i�h���t�g�j�����iHP / MP / COND�j
            // phase_base_delta�i�V�l / �ʏ� / �^�G���h�Ȃǂō����ւ��j
            // match_extra_delta�i�������̂݉��Z�j
            // veteran_extra_delta�i�x�e�����Ȃ���Z�j
            // ��b�����̑f�i�H���Ōy�������Ώہj
            // base_delta_for_food[s] = phase_base_delta[s] + match_extra_delta[s] + veteran_extra_delta[s]
            // s �� { HP, MP, COND}
            // ��POVERTY�̂݁A����Ƀy�i���e�B���u��b�����ɉ��Z�i�����j�v

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

            // 3 - B) �H���y���imitig�j���Z�o�iHP / MP / COND�j
            // mitig_total = plan.mitig_total
            // mitig_dist = menu �̔z���iHP / MP / COND���v = mitig_total�j
            // mitig[s] = mitig_dist[s]�i�K�v�Ȃ�CookingLv�̎厲�{�[�i�X�����Z�j
            var mitigHP = food.MitigHP;
            var mitigMP = food.MitigMP;
            var mitigCOND = food.MitigCOND;
            Debug.Log($"Mitigation - HP: {mitigHP}, MP: {mitigMP}, COND: {mitigCOND}");

            // 3 - C) �H���̓K�p�i�d�v�F�H���͉񕜂��Ȃ��j
            // �H���K�p��̃h���t�g
            // food_delta_applied[s] = min(0, base_delta_for_food[s] + mitig[s])
            // ����� 0 �Ȃ̂ŁA�H�������Ńv���X�񕜁iHP + �Ȃǁj�ɂ͂Ȃ�Ȃ�
            var foogDeltaAppliedHP = Math.Min(0, baseDeltaForFoodHP + mitigHP);
            var foogDeltaAppliedMP = Math.Min(0, baseDeltaForFoodMP + mitigMP);
            var foogDeltaAppliedCOND = Math.Min(0, baseDeltaForFoodCOND + mitigCOND);
            Debug.Log($"Food Delta Applied - HP: {foogDeltaAppliedHP}, MP: {foogDeltaAppliedMP}, COND: {foogDeltaAppliedCOND}");

            // 5) AB�����i�ʘg�B�����Ŋm��j
            // AB�́u�s���ɂ�鐬���␳�v�u�H���{���v�uCOND�i�K�{���v�����܂��B
            // base = ab_base_growth_per_month[phase]
            // raw = base + AB_growth_from_action�i���s���� growth_add�j
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
            // ���ӓ_�F
            // food_delta_applied ������̂� HP / MP / COND�̂�
            // T, RL, RP, CP �Ȃǂ� �H���ő��E����Ȃ��i�s�� / �C�x���g / �����ł̂ݓ����j
            var paramNextHP = foogDeltaAppliedHP + action.DeltaHP + _pendingEventMitigHP;
            var paramNextMP = foogDeltaAppliedMP + action.DeltaMP + _pendingEventMitigMP;
            var paramNextCOND = foogDeltaAppliedCOND + action.DeltaCOND + _pendingEventMitigCOND;
            _pendingEventMitigHP = 0;
            _pendingEventMitigMP = 0;
            _pendingEventMitigCOND = 0;

            var husband = GameSimulationManager.Instance.Husband;
            husband.ChangeHealth(paramNextHP);
            husband.ChangeMental(paramNextMP);
            husband.ChangeCondition(paramNextCOND);
            husband.GrowAbility(AbNext);

            if (parameterChangePanel != null && GameSimulationManager.Instance != null)
            {
                Dictionary<string, double> changes = new Dictionary<string, double>();
                changes["�̗�"] = paramNextHP;
                changes["�����^��"] = paramNextMP;
                changes["�R���f�B�V����"] = paramNextCOND;
                changes["�\�͒l"] = AbNext;

                if (changes.Count > 0)
                {
                    var tcs = new UniTaskCompletionSource();
                    currentParamTask = tcs;
                    parameterChangePanel.ShowChanges("�p�����[�^�ϓ�", changes);
                    await tcs.Task;
                    currentParamTask = null;
                }
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// Phase 5: �����񍐂̕\��
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

                string dateStr = $"{date.Year}�N {date.Month}��";
                int income = 0; // Calculate from budget history
                int expenses = (int)budget.FixedCost.TotalCost;
                int savings = (int)budget.CurrentSavings;

                string playerStats = $"�̗�: {husband.CurrentHealth}/{husband.MAX_HEALTH}\n" +
                                    $"�����^��: {husband.CurrentMental}/{husband.MAX_MENTAL}";

                string wifeStats = $"�P�A���x��: {wife.CareLevel}\n" +
                                  $"PR���x��: {wife.PRLevel}\n" +
                                  $"�R�[�`���x��: {wife.CoachLevel}";

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

        private EventModel PickRandomEvent()
        {
            var eventDataList = DataManager.Instance.GetDatas<EventModel>();
            if (eventDataList == null || eventDataList.Any() == false)
            {
                return null;
            }

            var eventData = eventDataList[UnityEngine.Random.Range(0, eventDataList.Count)];
            Debug.Log($"�����_���C�x���g����: {eventData.Title} (ID:{eventData.EventId})");
            return eventData;
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

            currentEventTask?.TrySetResult();
        }

        private void ApplyEventOptionEffects(List<EventOption> selectedOptions)
        {
            _pendingEventMitigHP = 0;
            _pendingEventMitigMP = 0;
            _pendingEventMitigCOND = 0;

            if (selectedOptions == null || selectedOptions.Count == 0)
            {
                return;
            }

            foreach (var option in selectedOptions)
            {
                if (option.CostMoney > 0)
                {
                    if (!GameSimulationManager.Instance.Budget.TrySpend(option.CostMoney))
                    {
                        Debug.LogWarning($"[GameTurnManager] Insufficient funds for event option cost: {option.CostMoney}");
                    }
                }
                else if (option.CostMoney < 0)
                {
                    GameSimulationManager.Instance.Budget.AddIncome(Mathf.Abs(option.CostMoney));
                }

                _pendingEventMitigHP += option.MitigHp;
                _pendingEventMitigMP += option.MitigMp;
                _pendingEventMitigCOND += option.MitigCond;
            }

            Debug.Log($"[GameTurnManager] Event option effects applied - CostMoney processed, MitigHP: {_pendingEventMitigHP}, MitigMP: {_pendingEventMitigMP}, MitigCOND: {_pendingEventMitigCOND}");
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
