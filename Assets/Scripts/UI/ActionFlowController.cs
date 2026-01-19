using Ambition.Data.Master;
using Ambition.UI.MainGame;
using UnityEngine;

namespace Ambition.UI
{
    /// <summary>
    /// 行動の詳細を表示し、実行・戻るボタンを管理するダイアログコントローラー
    /// </summary>
    public class ActionFlowController : MonoBehaviour
    {
        [SerializeField] private MainGameView mainGameView;

        [Header("UI Components")]
        [SerializeField] private SubMenuController subMenu;
        [SerializeField] private ActionInfoPanel actionInfoPanel;

        private void Awake()
        {
            if (subMenu != null)
            {
                subMenu.OnActionSelected += OnActionConfirmed;
                subMenu.OnBackPressed += OnSelectionBackPressed;
            }

            // 初期状態では非表示
            if (actionInfoPanel != null)
            {
                actionInfoPanel.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (subMenu != null)
            {
                subMenu.OnActionSelected -= OnActionConfirmed;
                subMenu.OnBackPressed -= OnSelectionBackPressed;
            }
        }

        /// <summary>
        /// メニューが選択された時の処理
        /// </summary>
        private void OnActionConfirmed(WifeActionModel wifeAction)
        {
            // 結果バブルを表示
            if (actionInfoPanel != null)
            {
                actionInfoPanel.ShowResult(wifeAction);
            }

            HideAllPanels();
        }

        /// <summary>
        /// 確認画面で"戻る"ボタンが押された時の処理
        /// </summary>
        private void OnSelectionBackPressed()
        {
        }

        /// <summary>
        /// すべてのパネルを非表示にする
        /// </summary>
        private void HideAllPanels()
        {
            if (subMenu != null)
            {
                subMenu.Close();
            }
        }
    }
}
