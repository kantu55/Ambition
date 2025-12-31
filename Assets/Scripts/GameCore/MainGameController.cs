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

        /// <summary>
        /// 保留中の行動（ユーザーが「実行」を選択した行動）
        /// </summary>
        private WifeActionModel pendingAction = null;

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

            // 確定ボタンのイベントを設定
            if (m_MainView.ConfirmButton != null)
            {
                m_MainView.ConfirmButton.onClick.AddListener(OnConfirmClicked);
                // 初期状態では無効化
                m_MainView.ConfirmButton.interactable = false;
            }

            // サブメニューコントローラーのイベントを設定
            if (m_MainView.SubMenuController != null)
            {
                m_MainView.SubMenuController.OnActionSelected += OnActionSelectedFromSubMenu;
                m_MainView.SubMenuController.OnBackPressed += OnSubMenuBackPressed;
            }

            // アクションダイアログコントローラーのイベントを設定
            if (m_MainView.ActionDialogController != null)
            {
                m_MainView.ActionDialogController.OnExecutePressed += OnActionExecutePressed;
                m_MainView.ActionDialogController.OnBackPressed += OnActionDialogBackPressed;
            }

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

        // --- サブメニュー関連 ---

        /// <summary>
        /// 指定されたカテゴリのサブメニューを開く
        /// </summary>
        private void OpenSubMenu(WifeActionModel.ActionMainCategory category)
        {
            if (DataManager.Instance == null || m_MainView.SubMenuController == null)
            {
                Debug.LogError("DataManager または SubMenuController が null です。");
                return;
            }

            var actions = DataManager.Instance.GetActionsByMainCategory(category);
            m_MainView.SubMenuController.Open(actions);
        }

        /// <summary>
        /// サブメニューから行動が選択された時
        /// </summary>
        private void OnActionSelectedFromSubMenu(WifeActionModel action)
        {
            Debug.Log($"行動選択: {action.Name}");
            if (m_MainView.ActionDialogController != null)
            {
                m_MainView.ActionDialogController.Open(action);
            }
        }

        /// <summary>
        /// サブメニューの戻るボタンが押された時
        /// </summary>
        private void OnSubMenuBackPressed()
        {
            Debug.Log("サブメニューを閉じます");
            if (m_MainView.SubMenuController != null)
            {
                m_MainView.SubMenuController.Close();
            }
        }

        // --- アクションダイアログ関連 ---

        /// <summary>
        /// アクションダイアログの実行ボタンが押された時
        /// </summary>
        private void OnActionExecutePressed(WifeActionModel action)
        {
            Debug.Log($"行動を保存: {action.Name}");
            pendingAction = action;

            // ダイアログとサブメニューを閉じる
            if (m_MainView.ActionDialogController != null)
            {
                m_MainView.ActionDialogController.Close();
            }

            if (m_MainView.SubMenuController != null)
            {
                m_MainView.SubMenuController.Close();
            }

            // 確定ボタンを有効化
            if (m_MainView.ConfirmButton != null)
            {
                m_MainView.ConfirmButton.interactable = true;
            }

            // 選択されたアクション情報を表示
            m_MainView.UpdateSelectedAction(action);
        }

        /// <summary>
        /// アクションダイアログの戻るボタンが押された時
        /// </summary>
        private void OnActionDialogBackPressed()
        {
            Debug.Log("アクションダイアログを閉じます");
            if (m_MainView.ActionDialogController != null)
            {
                m_MainView.ActionDialogController.Close();
            }
        }

        // --- 確定ボタン関連 ---

        /// <summary>
        /// 確定ボタンが押された時
        /// </summary>
        private void OnConfirmClicked()
        {
            if (pendingAction == null)
            {
                Debug.LogWarning("保留中の行動がありません。");
                return;
            }

            Debug.Log($"行動を実行: {pendingAction.Name}");

            // 行動を実行
            if (GameSimulationManager.Instance != null)
            {
                bool success = GameSimulationManager.Instance.ExecuteWifeAction(pendingAction);
                if (success)
                {
                    // 行動をクリア
                    pendingAction = null;

                    // 確定ボタンを無効化
                    if (m_MainView.ConfirmButton != null)
                    {
                        m_MainView.ConfirmButton.interactable = false;
                    }

                    // アクション情報パネルをクリア
                    m_MainView.UpdateSelectedAction(null);

                    // UIを更新
                    RefreshUI();
                }
                else
                {
                    Debug.LogWarning("行動の実行に失敗しました。");
                }
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

        private void OnDestroy()
        {
            // イベントリスナーのクリーンアップ
            if (m_MainView != null)
            {
                if (m_MainView.ConfirmButton != null)
                {
                    m_MainView.ConfirmButton.onClick.RemoveListener(OnConfirmClicked);
                }

                if (m_MainView.SubMenuController != null)
                {
                    m_MainView.SubMenuController.OnActionSelected -= OnActionSelectedFromSubMenu;
                    m_MainView.SubMenuController.OnBackPressed -= OnSubMenuBackPressed;
                }

                if (m_MainView.ActionDialogController != null)
                {
                    m_MainView.ActionDialogController.OnExecutePressed -= OnActionExecutePressed;
                    m_MainView.ActionDialogController.OnBackPressed -= OnActionDialogBackPressed;
                }
            }
        }
    }
}
