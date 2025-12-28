using Ambition.DataStructures;
using Ambition.UI;
using UnityEngine;

namespace Ambition.GameCore
{
    /// <summary>
    /// メイン画面のロジックを制御するコントローラー
    /// </summary>
    public class MainGameController : MonoBehaviour
    {
        [SerializeField] private MainGameView m_MainView;

        private WifeActionModel pendingAction;
        private WifeActionModel.ActionMainCategory currentCategory;

        private void Start()
        {
            if (GameSimulationManager.Instance != null)
            {
                GameSimulationManager.Instance.StartNewGame(1001);
            }

            m_MainView.BindButtons(
                OnSupportClicked,
                OnSelfPolishClicked,
                OnEnvironmentClicked,
                OnPRClicked
            );

            m_MainView.BindConfirmButton(OnConfirmClicked);

            // 確定ボタンは最初は無効
            m_MainView.SetConfirmButtonEnabled(false);

            RefreshUI();
        }

        // --- イベントハンドラ ---

        private void OnSupportClicked()
        {
            Debug.Log("コマンド: 夫を支える");
            OpenSubMenu(WifeActionModel.ActionMainCategory.SUPPORT_HUSBAND);
        }

        private void OnSelfPolishClicked()
        {
            Debug.Log("コマンド: 自分を磨く");
            OpenSubMenu(WifeActionModel.ActionMainCategory.SELF_POLISH);
        }

        private void OnEnvironmentClicked()
        {
            Debug.Log("コマンド: 環境を整える");
            OpenSubMenu(WifeActionModel.ActionMainCategory.ENVIRONMENT);
        }

        private void OnPRClicked()
        {
            Debug.Log("コマンド: 広報・営業");
            OpenSubMenu(WifeActionModel.ActionMainCategory.PR_SALES);
        }

        /// <summary>
        /// サブメニューを開く
        /// </summary>
        private void OpenSubMenu(WifeActionModel.ActionMainCategory category)
        {
            if (DataManager.Instance == null || m_MainView.SubMenuController == null)
            {
                Debug.LogError("DataManagerまたはSubMenuControllerが存在しません。");
                return;
            }

            currentCategory = category;
            var actions = DataManager.Instance.GetActionsByMainCategory(category);
            m_MainView.SubMenuController.Open(
                actions,
                OnActionSelected,
                OnSubMenuBack
            );
        }

        /// <summary>
        /// サブメニューでアクションが選択された時の処理
        /// </summary>
        private void OnActionSelected(WifeActionModel action)
        {
            if (m_MainView.ActionDialogController == null)
            {
                Debug.LogError("ActionDialogControllerが存在しません。");
                return;
            }

            m_MainView.ActionDialogController.Open(
                action,
                OnActionExecute,
                OnActionDialogBack
            );
        }

        /// <summary>
        /// アクションダイアログで実行ボタンが押された時の処理
        /// </summary>
        private void OnActionExecute(WifeActionModel action)
        {
            pendingAction = action;
            m_MainView.SetConfirmButtonEnabled(true);
            
            Debug.Log($"アクションを保存しました: {action.Name}");
        }

        /// <summary>
        /// アクションダイアログで戻るボタンが押された時の処理
        /// </summary>
        private void OnActionDialogBack()
        {
            // サブメニューを再表示
            OpenSubMenu(currentCategory);
        }

        /// <summary>
        /// サブメニューで戻るボタンが押された時の処理
        /// </summary>
        private void OnSubMenuBack()
        {
            // メイン画面に戻る（特に処理なし）
            Debug.Log("メイン画面に戻りました。");
        }

        /// <summary>
        /// 確定ボタンが押された時の処理
        /// </summary>
        private void OnConfirmClicked()
        {
            if (pendingAction == null)
            {
                Debug.LogWarning("保存されたアクションがありません。");
                return;
            }

            if (GameSimulationManager.Instance == null)
            {
                Debug.LogError("GameSimulationManagerが存在しません。");
                return;
            }

            // アクションを実行
            bool success = GameSimulationManager.Instance.ExecuteWifeAction(pendingAction);
            
            if (success)
            {
                Debug.Log($"アクションを実行しました: {pendingAction.Name}");
                
                // アクション実行後の処理
                pendingAction = null;
                m_MainView.SetConfirmButtonEnabled(false);
                RefreshUI();
            }
            else
            {
                Debug.LogWarning("アクションの実行に失敗しました。");
            }
        }

        // --- 画面更新 ---

        /// <summary>
        /// 現在のデータに基づいてViewを更新
        /// </summary>
        public void RefreshUI()
        {
            if (GameSimulationManager.Instance == null)
            {
                return;
            }

            m_MainView.RefreshView(GameSimulationManager.Instance.Date, GameSimulationManager.Instance.Budget, GameSimulationManager.Instance.Husband, GameSimulationManager.Instance.Wife);
        }
    }
}
