using Ambition.DataStructures;
using System;
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
        [SerializeField] private GameObject bubbleRoot;
        [SerializeField] private TextMeshProUGUI resultText;

        /// <summary>
        /// 確認ボタンが押された時のコールバック
        /// </summary>
        public event Action<WifeActionModel> OnConfirmPressed;

        private System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(128);

        private void Awake()
        {
            if (bubbleRoot != null)
            {
                bubbleRoot.SetActive(false);
            }
        }

        public void ShowResult(WifeActionModel wifeAction)
        {
            if (wifeAction == null)
            {
                return;
            }

            if (bubbleRoot != null)
            {
                bubbleRoot.SetActive(true);
            }

            if (resultText != null)
            {
                resultText.text = BuildResultText(wifeAction);
            }
        }

        public void HideResult()
        {
            if (bubbleRoot != null)
            {
                bubbleRoot.SetActive(false);
            }
        }

        private string BuildResultText(WifeActionModel wifeAction)
        {
            stringBuilder.Clear();
            stringBuilder.Append(wifeAction.Name);
            return stringBuilder.ToString();
        }
    }
}
