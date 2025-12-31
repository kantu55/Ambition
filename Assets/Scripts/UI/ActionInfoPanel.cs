using Ambition.DataStructures;
using TMPro;
using UnityEngine;

namespace Ambition.UI
{
    /// <summary>
    /// 選択されたアクションの情報を表示するパネル
    /// ボタンに付随して表示される
    /// </summary>
    public class ActionInfoPanel : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI actionNameText;
        [SerializeField] private TextMeshProUGUI costMoneyText;
        [SerializeField] private TextMeshProUGUI costWifeHealthText;

        private System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(128);

        private void Awake()
        {
            // 初期状態では非表示
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        /// <summary>
        /// アクション情報を表示
        /// </summary>
        /// <param name="action">表示するアクション</param>
        public void Show(WifeActionModel action)
        {
            if (action == null)
            {
                Hide();
                return;
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            UpdateContent(action);
        }

        /// <summary>
        /// パネルを非表示にする
        /// </summary>
        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        /// <summary>
        /// パネルの内容を更新
        /// </summary>
        private void UpdateContent(WifeActionModel action)
        {
            // アクション名
            if (actionNameText != null)
            {
                actionNameText.text = action.Name;
            }

            // コスト（資金）
            if (costMoneyText != null)
            {
                stringBuilder.Clear();
                stringBuilder.Append("資金: ");
                if (action.CostMoney > 0)
                {
                    stringBuilder.Append("-");
                }
                stringBuilder.Append(action.CostMoney.ToString("N0"));
                stringBuilder.Append("円");
                costMoneyText.text = stringBuilder.ToString();
            }

            // コスト（妻体力）
            if (costWifeHealthText != null)
            {
                stringBuilder.Clear();
                stringBuilder.Append("妻体力: ");
                if (action.CostWifeHealth > 0)
                {
                    stringBuilder.Append("-");
                }
                stringBuilder.Append(action.CostWifeHealth.ToString());
                costWifeHealthText.text = stringBuilder.ToString();
            }
        }
    }
}
