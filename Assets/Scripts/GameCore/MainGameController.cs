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
        [SerializeField] private MainGameView mainView;

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

            mainView.BindButtons(
                OnCareClicked,
                OnSupportClicked,
                OnSNSClicked,
                OnDisciplineClicked,
                OnTalkClicked,
                OnRestClicked
                );

            // 確定ボタンのイベントを設定
            if (mainView.ConfirmButton != null)
            {
                mainView.ConfirmButton.onClick.AddListener(OnConfirmClicked);
                // 初期状態では無効化
                mainView.ConfirmButton.interactable = false;
            }

            // サブメニューコントローラーのイベントを設定
            if (mainView.SubMenuController != null)
            {
                mainView.SubMenuController.OnActionSelected += OnActionSelectedFromSubMenu;
                mainView.SubMenuController.OnBackPressed += OnSubMenuBackPressed;
            }

            // アクションダイアログコントローラーのイベントを設定
            if (mainView.ActionDialogController != null)
            {
                mainView.ActionDialogController.OnExecutePressed += OnActionExecutePressed;
                mainView.ActionDialogController.OnBackPressed += OnActionDialogBackPressed;
            }

            RefreshUI();
        }

        // --- イベントハンドラ ---

        private void OnCareClicked()
        {
            Debug.Log("コマンド: ケア");
            OpenSubMenu(WifeActionModel.ActionMainCategory.CARE);
        }

        private void OnSupportClicked()
        {
            Debug.Log("コマンド: 練習支援");
            OpenSubMenu(WifeActionModel.ActionMainCategory.SUPPORT);
        }

        private void OnSNSClicked()
        {
            Debug.Log("コマンド: SNS/対外");
            OpenSubMenu(WifeActionModel.ActionMainCategory.SNS);
        }

        private void OnDisciplineClicked()
        {
            Debug.Log("コマンド: 躾");
            OpenSubMenu(WifeActionModel.ActionMainCategory.DISCIPLINE);
        }

        private void OnTalkClicked()
        {
            Debug.Log("コマンド: 夫婦仲");
            OpenSubMenu(WifeActionModel.ActionMainCategory.TALK);
        }

        private void OnRestClicked()
        {
            Debug.Log("コマンド: 休養");
            OpenSubMenu(WifeActionModel.ActionMainCategory.REST);
        }

        // --- サブメニュー関連 ---

        /// <summary>
        /// 指定されたカテゴリのサブメニューを開く
        /// </summary>
        private void OpenSubMenu(WifeActionModel.ActionMainCategory category)
        {
            if (DataManager.Instance == null || mainView.SubMenuController == null)
            {
                Debug.LogError("DataManager または SubMenuController が null です。");
                return;
            }

            var actions = DataManager.Instance.GetActionsByMainCategory(category);
            mainView.SubMenuController.Open(actions);
        }

        /// <summary>
        /// サブメニューから行動が選択された時
        /// </summary>
        private void OnActionSelectedFromSubMenu(WifeActionModel action)
        {
            Debug.Log($"行動選択: {action.Name}");
            if (mainView.ActionDialogController != null)
            {
                mainView.ActionDialogController.Open(action);
            }
        }

        /// <summary>
        /// サブメニューの戻るボタンが押された時
        /// </summary>
        private void OnSubMenuBackPressed()
        {
            Debug.Log("サブメニューを閉じます");
            if (mainView.SubMenuController != null)
            {
                mainView.SubMenuController.Close();
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
            if (mainView.ActionDialogController != null)
            {
                mainView.ActionDialogController.Close();
            }

            if (mainView.SubMenuController != null)
            {
                mainView.SubMenuController.Close();
            }

            // 確定ボタンを有効化
            if (mainView.ConfirmButton != null)
            {
                mainView.ConfirmButton.interactable = true;
            }

            // 選択されたアクション情報を表示
            mainView.UpdateSelectedAction(action);
        }

        /// <summary>
        /// アクションダイアログの戻るボタンが押された時
        /// </summary>
        private void OnActionDialogBackPressed()
        {
            Debug.Log("アクションダイアログを閉じます");
            if (mainView.ActionDialogController != null)
            {
                mainView.ActionDialogController.Close();
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
                    if (mainView.ConfirmButton != null)
                    {
                        mainView.ConfirmButton.interactable = false;
                    }

                    // アクション情報パネルをクリア
                    mainView.UpdateSelectedAction(null);

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

            mainView.RefreshView(GameSimulationManager.Instance.Date, GameSimulationManager.Instance.Budget, GameSimulationManager.Instance.Husband, GameSimulationManager.Instance.Wife, GameSimulationManager.Instance.Reputation);
        }

        private void OnDestroy()
        {
            // イベントリスナーのクリーンアップ
            if (mainView != null)
            {
                if (mainView.ConfirmButton != null)
                {
                    mainView.ConfirmButton.onClick.RemoveListener(OnConfirmClicked);
                }

                if (mainView.SubMenuController != null)
                {
                    mainView.SubMenuController.OnActionSelected -= OnActionSelectedFromSubMenu;
                    mainView.SubMenuController.OnBackPressed -= OnSubMenuBackPressed;
                }

                if (mainView.ActionDialogController != null)
                {
                    mainView.ActionDialogController.OnExecutePressed -= OnActionExecutePressed;
                    mainView.ActionDialogController.OnBackPressed -= OnActionDialogBackPressed;
                }
            }
        }
    }
}
