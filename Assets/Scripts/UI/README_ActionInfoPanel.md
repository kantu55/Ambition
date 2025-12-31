# ActionInfoPanel Setup Guide

## Overview
The ActionInfoPanel displays selected action details on main category buttons when an action is chosen but not yet confirmed.

## Unity Scene Setup

### Step 1: Create ActionInfoPanel GameObjects
For each main category button (4 total), create an ActionInfoPanel GameObject:

1. Create a child GameObject under or near each button:
   - `ActionInfoPanel_Support` (for "Support Husband" button)
   - `ActionInfoPanel_SelfPolish` (for "Self Polish" button)
   - `ActionInfoPanel_Environment` (for "Environment" button)
   - `ActionInfoPanel_PR` (for "PR/Sales" button)

2. Add the `ActionInfoPanel` component to each GameObject

### Step 2: Setup ActionInfoPanel Components
For each ActionInfoPanel, configure:

1. **Panel Root**: A parent GameObject that will be shown/hidden (can be the same GameObject)
2. **Action Name Text**: TextMeshProUGUI for displaying the action name
3. **Cost Money Text**: TextMeshProUGUI for displaying the money cost
4. **Cost Wife Health Text**: TextMeshProUGUI for displaying the wife health cost

### Step 3: Link to MainGameView
In the MainGameView component:

1. Assign each ActionInfoPanel to the corresponding serialized field:
   - `actionInfoSupport` → ActionInfoPanel_Support
   - `actionInfoSelfPolish` → ActionInfoPanel_SelfPolish
   - `actionInfoEnvironment` → ActionInfoPanel_Environment
   - `actionInfoPR` → ActionInfoPanel_PR

## Visual Style Recommendations
Based on the design image:
- Position the panel as an overlay on or near the button
- Use a semi-transparent background
- Apply a border or shadow for visibility
- Consider using anchors for responsive positioning

## Flow
1. User selects an action from the submenu
2. User clicks "Execute" in the action dialog
3. The ActionInfoPanel appears on the corresponding main category button
4. The Confirm button becomes active
5. User clicks "Confirm"
6. The action is executed and the panel disappears
7. The Confirm button becomes inactive

## Code Integration
The integration is complete in code:
- `MainGameController.OnActionExecutePressed` calls `MainGameView.UpdateSelectedAction` to show the panel
- `MainGameController.OnConfirmClicked` calls `MainGameView.UpdateSelectedAction(null)` to hide the panel
- The panel automatically selects the correct one based on the action's MainCategory
