using Ambition.Data.Master;
using Ambition.UI.MainGame;
using Ambition.GameCore;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Ambition.Core.Managers
{
    /// <summary>
    /// メイン画面のロジックを制御するコントローラー
    /// </summary>
    public class MainGameController : MonoBehaviour
    {
        [SerializeField] private MainGameView mainView;
        [SerializeField] private GameTurnManager gameTurnManager;

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
                mainView.SubMenuController.OnActionConfirmed += OnActionConfirmedFromSubMenu;
                mainView.SubMenuController.OnBackPressed += OnSubMenuBackPressed;
            }

            // GameTurnManagerのイベントを設定
            if (gameTurnManager != null)
            {
                gameTurnManager.OnTurnCompleted += OnTurnCompleted;
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

        /// <summary>
        /// サブメニューの確定ボタンが押された時
        /// </summary>
        private void OnActionConfirmedFromSubMenu(WifeActionModel action)
        {
            Debug.Log($"行動を保存: {action.Name}");
            pendingAction = action;

            // サブメニューを閉じる
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
            pendingAction = action;

            // サブメニューを閉じる
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
        }

        // --- 確定ボタン関連 ---

        /// <summary>
        /// 確定ボタンが押された時
        /// </summary>
        private async void OnConfirmClicked()
        {
            if (pendingAction == null)
            {
                Debug.LogWarning("保留中の行動がありません。");
                return;
            }

            Debug.Log($"行動を実行: {pendingAction.Name}");

            // 行動を実行 - GameTurnManagerに委譲
            if (gameTurnManager != null)
            {
                // 確定ボタンを無効化（処理中）
                if (mainView.ConfirmButton != null)
                {
                    mainView.ConfirmButton.interactable = false;
                }

                // GameTurnManagerを使用してターンを実行
                await gameTurnManager.ExecuteTurnAsync(pendingAction);
            }
            else
            {
                Debug.LogWarning("GameTurnManager が見つかりません。");
            }
        }

        /// <summary>
        /// ターン完了時の処理
        /// </summary>
        private void OnTurnCompleted()
        {
            // 行動をクリア
            pendingAction = null;

            // アクション情報パネルをクリア
            mainView.UpdateSelectedAction(null);
            mainView.ResetAllPreviews();

            // UIを更新
            RefreshUI();

            // 確定ボタンを再度有効化
            if (mainView.ConfirmButton != null)
            {
                mainView.ConfirmButton.interactable = false;
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
                    mainView.SubMenuController.OnActionConfirmed -= OnActionConfirmedFromSubMenu;
                }
            }

            if (gameTurnManager != null)
            {
                gameTurnManager.OnTurnCompleted -= OnTurnCompleted;
            }
        }
    }
}
