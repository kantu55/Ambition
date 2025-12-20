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

            RefreshUI();
        }

        // --- イベントハンドラ ---

        private void OnSupportClicked()
        {
            Debug.Log("コマンド: 夫を支える");
            // TODO: 「夫を支える」サブメニュー(WifeActionsリスト)を表示する処理へ
            // var actions = DataManager.Instance.GetActionsByMainCategory(WifeActionModel.ActionMainCategory.SUPPORT_HUSBAND);
            // SubMenuController.Open(actions);
        }

        private void OnSelfPolishClicked()
        {
            Debug.Log("コマンド: 自分を磨く");
            // TODO: 自分磨きサブメニューへ
        }

        private void OnEnvironmentClicked()
        {
            Debug.Log("コマンド: 環境を整える");
            // TODO: 環境サブメニューへ
        }

        private void OnPRClicked()
        {
            Debug.Log("コマンド: 広報・営業");
            // TODO: 広報サブメニューへ
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

            m_MainView.RefreshView(GameSimulationManager.Instance.Husband, GameSimulationManager.Instance.Wife);
        }
    }
}
