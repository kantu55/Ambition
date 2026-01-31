using System;
using System.Collections.Generic;
using Ambition.Data.Master;
using Ambition.UI.Panels;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Ambition.GameCore
{
    /// <summary>
    /// Orchestrates the game turn flow using UniTask.
    /// Flow: Action Selection → Match Results → Event → Parameter Changes → Monthly Report → Next Turn
    /// </summary>
    public class GameTurnManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private MatchResultPanel matchResultPanel;
        [SerializeField] private EventDialogPanel eventDialogPanel;
        [SerializeField] private ParameterChangePanel parameterChangePanel;
        [SerializeField] private MonthlyReportPanel monthlyReportPanel;

        /// <summary>
        /// Event fired when a turn completes
        /// </summary>
        public event Action OnTurnCompleted;

        /// <summary>
        /// Event fired when the turn flow starts
        /// </summary>
        public event Action OnTurnFlowStarted;

        private bool isProcessingTurn = false;

        private void Awake()
        {
            // Subscribe to panel events
            if (matchResultPanel != null)
            {
                matchResultPanel.OnContinueClicked += OnMatchResultContinue;
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

        private void OnDestroy()
        {
            // Unsubscribe from panel events
            if (matchResultPanel != null)
            {
                matchResultPanel.OnContinueClicked -= OnMatchResultContinue;
            }

            if (eventDialogPanel != null)
            {
                eventDialogPanel.OnEventConfirmed -= OnEventConfirmed;
                eventDialogPanel.OnEventCancelled -= OnEventCancelled;
            }

            if (parameterChangePanel != null)
            {
                parameterChangePanel.OnContinueClicked -= OnParameterChangeContinue;
            }

            if (monthlyReportPanel != null)
            {
                monthlyReportPanel.OnContinueClicked -= OnMonthlyReportContinue;
            }
        }

        /// <summary>
        /// Execute a turn with the given action
        /// </summary>
        /// <param name="action">The wife action to execute</param>
        public async UniTask ExecuteTurnAsync(WifeActionModel action)
        {
            if (isProcessingTurn)
            {
                Debug.LogWarning("[GameTurnManager] Already processing a turn");
                return;
            }

            isProcessingTurn = true;
            OnTurnFlowStarted?.Invoke();

            try
            {
                Debug.Log($"[GameTurnManager] Starting turn execution: {action.Name}");

                // Phase 1: Process action and apply effects
                ProcessActionPhase(action);

                // Phase 2: Show match results (if applicable)
                await ShowMatchResultPhaseAsync();

                // Phase 3: Show random event (if any)
                await ShowEventPhaseAsync();

                // Phase 4: Show parameter changes
                await ShowParameterChangePhaseAsync(action);

                // Phase 5: Show monthly report
                await ShowMonthlyReportPhaseAsync();

                // Phase 6: Finalize turn (increment turn counter, etc.)
                FinalizeTurn();

                Debug.Log("[GameTurnManager] Turn execution completed");
                OnTurnCompleted?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameTurnManager] Error during turn execution: {e.Message}");
            }
            finally
            {
                isProcessingTurn = false;
            }
        }

        /// <summary>
        /// Phase 1: Process the action and apply immediate effects
        /// </summary>
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
        /// Phase 2: Show match results (if applicable for this turn/month)
        /// </summary>
        private async UniTask ShowMatchResultPhaseAsync()
        {
            Debug.Log("[GameTurnManager] Phase 2: Match results");

            // Check if there's a scheduled match this month
            // For now, skip if no match is scheduled
            // In the future, check schedule and show match results

            await UniTask.Yield();
        }

        /// <summary>
        /// Phase 3: Show random event (if any)
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

        private UniTaskCompletionSource currentEventTask;
        private UniTaskCompletionSource currentMatchTask;
        private UniTaskCompletionSource currentParamTask;
        private UniTaskCompletionSource currentReportTask;

        /// <summary>
        /// Phase 4: Show parameter changes
        /// </summary>
        private async UniTask ShowParameterChangePhaseAsync(WifeActionModel action)
        {
            Debug.Log("[GameTurnManager] Phase 4: Parameter changes");

            if (parameterChangePanel != null && GameSimulationManager.Instance != null)
            {
                // Collect parameter changes
                Dictionary<string, int> changes = new Dictionary<string, int>();

                if (action.DeltaHP != 0)
                {
                    changes["体力"] = action.DeltaHP;
                }

                if (action.DeltaMP != 0)
                {
                    changes["メンタル"] = action.DeltaMP;
                }

                if (action.CashCost != 0)
                {
                    changes["予算"] = -action.CashCost;
                }

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
        /// Phase 5: Show monthly report
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
                int expenses = budget.FixedCost.TotalFixedCost;
                int savings = budget.CurrentSavings;

                string playerStats = $"体力: {husband.CurrentHP}/{husband.MaxHP}\n" +
                                    $"メンタル: {husband.CurrentMP}/{husband.MaxMP}";

                string wifeStats = $"愛情: {wife.Affection}\n" +
                                  $"ストレス: {wife.Stress}";

                var tcs = new UniTaskCompletionSource();
                currentReportTask = tcs;

                monthlyReportPanel.ShowReport(dateStr, income, expenses, savings, playerStats, wifeStats);

                await tcs.Task;
                currentReportTask = null;
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// Phase 6: Finalize the turn
        /// </summary>
        private void FinalizeTurn()
        {
            Debug.Log("[GameTurnManager] Phase 6: Finalizing turn");

            if (GameSimulationManager.Instance == null)
            {
                return;
            }

            // Advance to next month
            GameSimulationManager.Instance.Date.AdvanceMonth();

            // Pay monthly fixed costs
            GameSimulationManager.Instance.Budget.PayMonthlyFixedCosts();

            // Update status ailments, injuries, etc.
            // (These methods already exist in GameSimulationManager but are called from ExecuteWifeAction)

            Debug.Log($"[GameTurnManager] Turn finalized. New date: {GameSimulationManager.Instance.Date.Year}年 {GameSimulationManager.Instance.Date.Month}月");
        }

        /// <summary>
        /// Pick a random event (if any should occur)
        /// </summary>
        private EventModel PickRandomEvent()
        {
            var eventDataList = Core.Managers.DataManager.Instance?.GetDatas<EventModel>();
            if (eventDataList == null || eventDataList.Count == 0)
            {
                return null;
            }

            // 20% chance of random event occurring
            if (UnityEngine.Random.value > 0.2f)
            {
                return null;
            }

            int randomIndex = UnityEngine.Random.Range(0, eventDataList.Count);
            return eventDataList[randomIndex];
        }

        // Event handlers for panels
        private void OnMatchResultContinue()
        {
            currentMatchTask?.TrySetResult();
        }

        private void OnEventConfirmed(EventModel eventData)
        {
            // Apply event effects
            if (eventData != null && GameSimulationManager.Instance != null)
            {
                if (eventData.DeltaHP != 0)
                {
                    GameSimulationManager.Instance.Husband.ChangeHealth(eventData.DeltaHP);
                }

                if (eventData.DeltaMP != 0)
                {
                    GameSimulationManager.Instance.Husband.ChangeMental(eventData.DeltaMP);
                }

                if (eventData.DeltaBudget != 0)
                {
                    if (eventData.DeltaBudget > 0)
                    {
                        GameSimulationManager.Instance.Budget.AddIncome(eventData.DeltaBudget);
                    }
                    else
                    {
                        GameSimulationManager.Instance.Budget.TrySpend(-eventData.DeltaBudget);
                    }
                }
            }

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
