# Meal Screen Flow Setup Guide

## Overview
The Meal Screen Flow implements a three-step process for selecting and confirming meal choices:
1. **Tier Selection (①食事_ティア選択)**: Display Tiers 1-4 and current food expenses
2. **Menu Selection (②食事_メニュー選択)**: Show specific meals for selected tier with details
3. **Confirmation Dialog (③食事_確認ダイアログ)**: Confirm meal selection and show result

## Architecture

### Components
- **MealFlowController**: Main manager coordinating the entire meal flow
- **MealTierPanel**: Handles Tier 1-4 selection and displays food expenses
- **MealSelectionPanel**: Shows meal menu list and details for selected tier
- **MealConfirmPanel**: Displays confirmation dialog with meal details
- **MealResultBubble**: Shows popup with selected meal info after confirmation

## Unity Scene Setup

### Step 1: Create Main Structure
1. Create a GameObject named `MealFlowManager`
2. Add the `MealFlowController` component to it
3. Create child GameObjects for each panel:
   - `TierSelectionPanel`
   - `MenuSelectionPanel`
   - `ConfirmationPanel`
   - `ResultBubble`

### Step 2: Setup MealTierPanel
1. Add `MealTierPanel` component to `TierSelectionPanel`
2. Configure components:
   - **Panel Root**: Parent GameObject for the panel
   - **Tier Button Container**: Transform for button list (use ScrollRect/VerticalLayoutGroup)
   - **Tier Button Prefab**: Button prefab for tier buttons
   - **Back Button**: Button to return to home
   - **Expenses Panel Root**: GameObject for expenses info display
   - **Expenses Text**: TextMeshProUGUI for showing current food costs

### Step 3: Setup MealSelectionPanel
1. Add `MealSelectionPanel` component to `MenuSelectionPanel`
2. Configure components:
   - **Panel Root**: Parent GameObject for the panel
   - **Menu Button Container**: Transform for menu buttons (use ScrollRect/VerticalLayoutGroup)
   - **Menu Button Prefab**: Button prefab for menu items
   - **Back Button**: Button to return to tier selection
   - **Details Panel Root**: GameObject for right panel details
   - **Menu Name Text**: TextMeshProUGUI for menu name
   - **Menu Image**: Image component for menu visual (optional)
   - **Menu Effect Text**: TextMeshProUGUI for effect details (HP, MP, COND)

### Step 4: Setup MealConfirmPanel
1. Add `MealConfirmPanel` component to `ConfirmationPanel`
2. Configure components:
   - **Panel Root**: Parent GameObject for the panel
   - **Confirm Text**: TextMeshProUGUI showing "この食事を選択しますか？"
   - **Menu Name Text**: TextMeshProUGUI for menu name
   - **Menu Image**: Image component for menu visual (optional)
   - **Menu Effect Text**: TextMeshProUGUI for effect details
   - **Confirm Button**: Button to confirm selection
   - **Back Button**: Button to return to menu selection

### Step 5: Setup MealResultBubble
1. Add `MealResultBubble` component to `ResultBubble`
2. Configure components:
   - **Bubble Root**: GameObject for the popup bubble
   - **Result Text**: TextMeshProUGUI for result message
   - **Display Duration**: Float value (default 3.0 seconds)
3. Position the bubble above the main button area

### Step 6: Link to MealFlowController
1. In the `MealFlowController` component:
   - **Tier Panel**: Assign TierSelectionPanel's MealTierPanel
   - **Selection Panel**: Assign MenuSelectionPanel's MealSelectionPanel
   - **Confirm Panel**: Assign ConfirmationPanel's MealConfirmPanel
   - **Result Bubble**: Assign ResultBubble's MealResultBubble
   - **Meal Menu Button**: Assign the "食事" (Meal) button from main UI

### Step 7: Initialize in Game
In your game initialization code (e.g., MainGameController):
```csharp
// Get reference to MealFlowController
MealFlowController mealFlow = GetComponent<MealFlowController>();

// Set RuntimeFixedCost reference
mealFlow.SetRuntimeFixedCost(runtimeFixedCost);

// Initialize data
mealFlow.Initialize();
```

## Data Requirements

### FoodModel (CSV: Food)
- Required columns: id, name, monthly_cost
- Represents Tier levels and their base costs
- Example rows:
  - id=1, name="Tier 1", monthly_cost=30000
  - id=2, name="Tier 2", monthly_cost=50000
  - id=3, name="Tier 3", monthly_cost=80000
  - id=4, name="Tier 4", monthly_cost=120000

### FoodMitModel (CSV: FoodMit)
- Required columns: tier, menu_id, menu_name, menu_type, mitig_HP, mitig_MP, mitig_COND
- Represents specific meal items for each tier
- Example rows:
  - tier="1", menu_id="1_1", menu_name="定食A", mitig_HP=5, mitig_MP=3, mitig_COND=0
  - tier="1", menu_id="1_2", menu_name="定食B", mitig_HP=3, mitig_MP=5, mitig_COND=0

## Flow Diagram
```
[Meal Menu Button] 
    ↓
①[Tier Selection Panel]
    - Shows Tiers 1-4
    - Shows Current Food Expenses
    - [Back] → Home
    ↓ (Select Tier)
②[Menu Selection Panel]
    - Shows Meal List for Tier
    - Shows Menu Details (Right Panel)
    - [Back] → Tier Selection
    ↓ (Select Menu)
③[Confirmation Panel]
    - Shows Menu Details
    - "この食事を選択しますか？"
    - [Confirm] → Execute & Show Result Bubble
    - [Back] → Menu Selection
    ↓ (Confirm)
[Result Bubble]
    - Shows selected meal info
    - Auto-hides after 3 seconds
    ↓
[Return to Home]
```

## Visual Style Recommendations
- Use consistent panel backgrounds matching game aesthetic
- Tier buttons should be clearly labeled (Tier 1, Tier 2, etc.)
- Menu selection should show grid or list layout
- Confirmation dialog should be centered and modal
- Result bubble should be positioned above main UI for visibility
- Use transitions/animations for smooth panel switching

## Notes
- All panels are initially hidden
- Only one panel is active at a time
- Back button navigation maintains state
- Menu images are optional and need to be loaded separately
- The flow automatically manages panel visibility
- RuntimeFixedCost is used to display current food expenses
