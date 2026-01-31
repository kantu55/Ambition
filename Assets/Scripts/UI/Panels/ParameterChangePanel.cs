using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI.Panels
{
    /// <summary>
    /// Panel for visualizing parameter/stat changes (HP, MP, Budget, etc.)
    /// </summary>
    public class ParameterChangePanel : BaseGamePanel
    {
        [Header("Parameter Change UI")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform changesContainer;
        [SerializeField] private GameObject changeItemPrefab;
        [SerializeField] private Button continueButton;

        /// <summary>
        /// Event fired when the user clicks continue
        /// </summary>
        public event Action OnContinueClicked;

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

        /// <summary>
        /// Show parameter changes
        /// </summary>
        /// <param name="title">Panel title (e.g., "パラメータ変動")</param>
        /// <param name="changes">Dictionary of parameter names and their changes</param>
        public void ShowChanges(string title, Dictionary<string, int> changes)
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

        /// <summary>
        /// Create a change item display
        /// </summary>
        private void CreateChangeItem(string paramName, int changeValue)
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

        private string FormatChange(int value)
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
