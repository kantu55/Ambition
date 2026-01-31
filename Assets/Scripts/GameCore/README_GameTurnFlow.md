# Game Turn Flow System

## Overview

This document describes the new game turn flow system implemented using `GameTurnManager` and `UniTask`. The system avoids frequent scene transitions by using UI panels within the `MainGameScene` for a smooth gameplay experience.

## Architecture

### GameTurnManager

Located at: `Assets/Scripts/GameCore/GameTurnManager.cs`

The `GameTurnManager` orchestrates the entire turn flow using async/await pattern with UniTask. It manages the following phases:

1. **Action Phase**: Process the selected wife action and apply immediate effects
2. **Match Results Phase**: Display match results (if applicable for the current month)
3. **Event Phase**: Show random events and apply their effects
4. **Parameter Change Phase**: Visualize stat changes from the action
5. **Monthly Report Phase**: Display end-of-month summary
6. **Finalize Phase**: Advance to next month and handle monthly processing

### UI Panels

All panels inherit from the abstract base class `BaseGamePanel` located at `Assets/Scripts/UI/Panels/BaseGamePanel.cs`.

#### BaseGamePanel

Provides common functionality for all game panels:
- `Show()`: Display the panel
- `Hide()`: Hide the panel
- `Close()`: Hide and notify listeners
- `OnPanelClosed`: Event fired when panel closes
- `IsVisible`: Property indicating panel visibility

#### Panel Components

1. **MatchResultPanel** (`MatchResultPanel.cs`)
   - Displays match results (game performance, stats changes)
   - Shows: Match title, result description, stats changes
   - User clicks "Continue" to proceed

2. **EventDialogPanel** (`EventDialogPanel.cs`)
   - Displays random events and dialogs
   - Shows: Event title, description, effects (HP, MP, Budget changes)
   - Applies event effects when confirmed
   - User can confirm or cancel (if applicable)

3. **ParameterChangePanel** (`ParameterChangePanel.cs`)
   - Visualizes stat changes from actions
   - Dynamically creates UI elements for each parameter
   - Shows changes with appropriate colors (green for positive, red for negative)
   - Displays: HP, MP, Budget, and other stat changes

4. **MonthlyReportPanel** (`MonthlyReportPanel.cs`)
   - Shows end-of-month summaries
   - Displays: Date, income, expenses, savings
   - Shows player stats (HP, MP, evaluation)
   - Shows wife stats (affection, stress)
   - User clicks "Continue" to proceed to next turn

## Integration with MainGameController

The `MainGameController` has been updated to delegate turn execution to `GameTurnManager`:

```csharp
// In MainGameController.cs
[SerializeField] private GameTurnManager gameTurnManager;

private async void OnConfirmClicked()
{
    // Delegate to GameTurnManager
    await gameTurnManager.ExecuteTurnAsync(pendingAction);
}

private void OnTurnCompleted()
{
    // Clean up and refresh UI after turn completes
    RefreshUI();
}
```

## Flow Diagram

```
User selects action
    ↓
User clicks "Confirm"
    ↓
GameTurnManager.ExecuteTurnAsync()
    ↓
[Phase 1] Process Action
    - Consume resources (money, stamina)
    - Apply immediate effects to player/wife
    ↓
[Phase 2] Show Match Results (if applicable)
    - MatchResultPanel displays
    - User clicks Continue
    ↓
[Phase 3] Show Random Event (20% chance)
    - EventDialogPanel displays
    - User confirms/cancels
    - Event effects applied
    ↓
[Phase 4] Show Parameter Changes
    - ParameterChangePanel displays
    - Shows all stat changes from action
    - User clicks Continue
    ↓
[Phase 5] Show Monthly Report
    - MonthlyReportPanel displays
    - Shows summary of month
    - User clicks Continue
    ↓
[Phase 6] Finalize Turn
    - Advance to next month
    - Pay monthly fixed costs
    - Update game state
    ↓
OnTurnCompleted event fires
    ↓
MainGameController refreshes UI
```

## Usage in Unity Editor

### Setup Required

1. **In MainGameScene**:
   - Add `GameTurnManager` component to a GameObject
   - Reference the following UI panels:
     - `MatchResultPanel`
     - `EventDialogPanel`
     - `ParameterChangePanel`
     - `MonthlyReportPanel`

2. **In MainGameController**:
   - Reference the `GameTurnManager` component
   - The controller will automatically wire up event handlers

3. **Create UI Panel GameObjects**:
   - Create GameObjects for each panel type
   - Add the corresponding panel component
   - Design UI elements (TextMeshProUGUI, Buttons, etc.)
   - Reference UI elements in the panel's inspector fields

### Panel UI Requirements

Each panel needs the following UI elements (referenced in SerializeField):

**MatchResultPanel**:
- `matchTitleText` (TextMeshProUGUI)
- `resultDescriptionText` (TextMeshProUGUI)
- `statsChangeText` (TextMeshProUGUI)
- `continueButton` (Button)

**EventDialogPanel**:
- `eventTitleText` (TextMeshProUGUI)
- `eventDescriptionText` (TextMeshProUGUI)
- `eventEffectsText` (TextMeshProUGUI)
- `confirmButton` (Button)
- `cancelButton` (Button) - optional

**ParameterChangePanel**:
- `titleText` (TextMeshProUGUI)
- `changesContainer` (Transform) - parent for change items
- `changeItemPrefab` (GameObject) - prefab with 2 TextMeshProUGUI components
- `continueButton` (Button)

**MonthlyReportPanel**:
- `reportTitleText` (TextMeshProUGUI)
- `dateText` (TextMeshProUGUI)
- `incomeText` (TextMeshProUGUI)
- `expensesText` (TextMeshProUGUI)
- `savingsText` (TextMeshProUGUI)
- `playerStatsText` (TextMeshProUGUI)
- `wifeStatsText` (TextMeshProUGUI)
- `summaryText` (TextMeshProUGUI)
- `continueButton` (Button)

## Benefits

1. **No Scene Transitions**: All turn flow happens within MainGameScene
2. **Smooth UX**: Async/await with UniTask provides seamless flow
3. **Maintainable**: Clear separation of concerns with dedicated panels
4. **Extensible**: Easy to add new phases or modify existing ones
5. **Reusable**: Base panel class can be extended for other UI needs

## Future Enhancements

- Add animations/transitions between phases
- Implement skip functionality for experienced players
- Add configurable phase ordering
- Support for branching/conditional phases
- Save/load support for mid-turn states
- Analytics tracking for player decisions
