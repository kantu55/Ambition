using Ambition.Data.Master;
using Ambition.Data.Master.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI.Panels
{
    /// <summary>
    /// マッチ結果パネルのクラス
    /// </summary>
    public class EventDialogPanel : BaseGamePanel
    {
        [SerializeField] private TextMeshProUGUI eventTitleText;
        [SerializeField] private TextMeshProUGUI eventDescriptionText;
        [SerializeField] private TextMeshProUGUI eventEffectsText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        public event System.Action<EventMaster> OnEventConfirmed;
        public event System.Action OnEventCancelled;

        private EventMaster currentEvent;

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


        public void ShowEvent(EventMaster eventModel)
        {
            if (eventModel == null)
            {
                Debug.LogWarning("[EventDialogPanel]イベントデータがない");
                return;
            }

            currentEvent = eventModel;
            Show();

            if (eventTitleText != null)
            {
                eventTitleText.SetText(eventModel.Title);
            }

            if (eventDescriptionText != null)
            {
                eventDescriptionText.SetText(eventModel.EventType.ToString());
            }

            if (eventEffectsText != null)
            {
                //eventEffectsText.SetText(eventModel.Effect);
            }

            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(true);
            }

            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(false);
            }
        }

        private string BuildEffectsText(EventModel eventData)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // データから効果を取得して表示

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