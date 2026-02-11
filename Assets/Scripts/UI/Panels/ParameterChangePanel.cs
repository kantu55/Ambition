using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI.Panels
{
    /// <summary>
    /// パラメータ変動パネルのクラス
    /// </summary>
    public class ParameterChangePanel : BaseGamePanel
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform changesContainer;
        [SerializeField] private GameObject changeItemPrefab;
        [SerializeField] private Button continueButton;

        public event System.Action OnContinueClicked;

        private List<GameObject> instantiatedItems = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(HandleContinueClicked);
            }
        }

        private void OnDestroy()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(HandleContinueClicked);
            }
        }

        public void ShowChanges(string title, Dictionary<string, double> changes)
        {
            Show();

            if (titleText != null)
            {
                titleText.text = title;
            }

            ClearChangeItems();

            if (changes != null && changesContainer != null && changeItemPrefab != null)
            {
                foreach (var change in changes)
                {
                    CreateChangeItem(change.Key, change.Value);
                }
            }
        }

        private void CreateChangeItem(string paramName, double changeValue)
        {
            GameObject item = Instantiate(changeItemPrefab, changesContainer);
            instantiatedItems.Add(item);

            // Find text components in the item
            TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                // First text is parameter name
                texts[0].text = paramName;

                // Second text is change value
                string changeText = FormatChange(changeValue);
                texts[1].text = changeText;

                // Color based on positive/negative
                texts[1].color = changeValue >= 0 ? Color.green : Color.red;
            }
            else if (texts.Length == 1)
            {
                // Fallback: single text shows both
                texts[0].text = $"{paramName}: {FormatChange(changeValue)}";
            }
        }

        private string FormatChange(double value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        private void ClearChangeItems()
        {
            foreach (var item in instantiatedItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }

            instantiatedItems.Clear();
        }

        private void HandleContinueClicked()
        {
            OnContinueClicked?.Invoke();
            Hide();
        }

        protected override void OnHide()
        {
            base.OnHide();
            ClearChangeItems();
        }
    }
}