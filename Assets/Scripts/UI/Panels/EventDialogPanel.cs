using System;
using Ambition.Data.Master;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI.Panels
{
    /// <summary>
    /// Panel for displaying random events and dialog interactions
    /// </summary>
    public class EventDialogPanel : BaseGamePanel
    {
        [Header("Event Dialog UI")]
        [SerializeField] private TextMeshProUGUI eventTitleText;
        [SerializeField] private TextMeshProUGUI eventDescriptionText;
        [SerializeField] private TextMeshProUGUI eventEffectsText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        /// <summary>
        /// Event fired when the user confirms the event
        /// </summary>
        public event Action<EventModel> OnEventConfirmed;

        /// <summary>
        /// Event fired when the user cancels/closes the event
        /// </summary>
        public event Action OnEventCancelled;

        private EventModel currentEvent;

        protected override void Awake()
        {
            base.Awake();

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(HandleConfirmClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(HandleCancelClicked);
            }
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(HandleConfirmClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(HandleCancelClicked);
            }
        }

        /// <summary>
        /// Display an event dialog
        /// </summary>
        /// <param name="eventData">Event data to display</param>
        public void ShowEvent(EventModel eventData)
        {
            if (eventData == null)
            {
                Debug.LogWarning("[EventDialogPanel] Event data is null");
                return;
            }

            currentEvent = eventData;
            Show();

            if (eventTitleText != null)
            {
                eventTitleText.text = eventData.Title;
            }

            if (eventDescriptionText != null)
            {
                eventDescriptionText.text = eventData.Description;
            }

            if (eventEffectsText != null)
            {
                eventEffectsText.text = BuildEffectsText(eventData);
            }

            // Show/hide buttons based on event type
            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(true);
            }

            // Only show cancel button if event has choices
            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Build the effects text from event data
        /// </summary>
        private string BuildEffectsText(EventModel eventData)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (eventData.DeltaHP != 0)
            {
                sb.AppendLine($"体力: {FormatChange(eventData.DeltaHP)}");
            }

            if (eventData.DeltaMP != 0)
            {
                sb.AppendLine($"メンタル: {FormatChange(eventData.DeltaMP)}");
            }

            if (eventData.DeltaBudget != 0)
            {
                sb.AppendLine($"予算: {FormatChange(eventData.DeltaBudget)}円");
            }

            return sb.ToString();
        }

        private string FormatChange(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        private void HandleConfirmClicked()
        {
            OnEventConfirmed?.Invoke(currentEvent);
            Hide();
        }

        private void HandleCancelClicked()
        {
            OnEventCancelled?.Invoke();
            Hide();
        }
    }
}
