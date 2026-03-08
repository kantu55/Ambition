using Ambition.Data.Master;
using Ambition.Data.Master.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem.iOS;
using Ambition.Core.Managers;
using System.Linq;

namespace Ambition.UI.Panels
{
    /// <summary>
    /// マッチ結果パネルのクラス
    /// </summary>
    public class EventDialogPanel : BaseGamePanel
    {
        private const int SPEAKER_HUSBAND = 1;
        private const int SPEAKER_WIFE = 2;

        [SerializeField] private TextMeshProUGUI eventTitleText;
        [SerializeField] private TextMeshProUGUI eventSpeakerText;
        [SerializeField] private TextMeshProUGUI eventMessageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        [Header("Options")]
        [SerializeField] private Transform optionGroup;
        [SerializeField] private Button optionButtonPrefab;

        public event System.Action<EventMaster> OnEventConfirmed;
        public event System.Action OnEventCancelled;

        private EventMaster currentEvent;
        private UniTaskCompletionSource nextTcs;
        private UniTaskCompletionSource<EventOption> optionTcs;

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

        public async UniTask<List<EventOption>> ShowEventAsync(EventMaster eventData)
        {
            var selectedOptions = new List<EventOption>();

            if (eventData == null)
            {
                return selectedOptions;
            }

            currentEvent = eventData;
            if (eventTitleText != null)
            {
                eventTitleText.enabled = true;
                eventTitleText.SetText(eventData.Title);
            }

            Show();

            var allDialogs = DataManager.Instance.GetDatas<EventDialog>();
            var allOptions = DataManager.Instance.GetDatas<EventOption>();

            int currentGroupId = eventData.FirstDialogGroupId;

            while (currentGroupId != 0)
            {
                var dialogs = allDialogs.Where(v => v.DialogGroupId == currentGroupId).OrderBy(vv => vv.PageNumber).ToList();
                if (dialogs.Count == 0)
                {
                    break;
                }

                int nextGroupId = 0;

                foreach (var dialog in dialogs)
                {
                    ShowDialog(dialog);

                    if (dialog.OptionGroupId > 0)
                    {
                        var options = allOptions.Where(v => v.OptionGroupId == dialog.OptionGroupId).ToList();
                        if (confirmButton != null)
                        {
                            confirmButton.gameObject.SetActive(false);
                        }

                        optionTcs = new UniTaskCompletionSource<EventOption>();
                        ShowOptions(options, selectedOption =>
                        {
                            ClearOptions();
                            optionTcs?.TrySetResult(selectedOption);
                        });

                        var selectedOption = await optionTcs.Task;
                        optionTcs = null;

                        selectedOptions.Add(selectedOption);
                        nextGroupId = selectedOption.NextDialogGroupId;
                        break;
                    }
                    else
                    {
                        if (confirmButton != null)
                        {
                            confirmButton.gameObject.SetActive(true);
                        }

                        nextTcs = new UniTaskCompletionSource();
                        await nextTcs.Task; // 確認ボタンが押されるまで待機
                        nextTcs = null;
                    }
                }

                currentGroupId = nextGroupId;
            }

            HideAll();
            OnEventConfirmed?.Invoke(eventData);
            return selectedOptions;
        }

        public void ShowDialog(EventDialog dialogData)
        {
            eventSpeakerText.enabled = true;
            eventMessageText.enabled = true;

            switch (dialogData.SpeakerId)
            {
                case SPEAKER_HUSBAND:
                    eventSpeakerText.SetText("夫");
                    break;

                case SPEAKER_WIFE:
                    eventSpeakerText.SetText("妻");
                    break;
            }

            eventMessageText.SetText(dialogData.Text);
        }

        public void ShowOptions(List<EventOption> options, System.Action<EventOption> onOptionSelected)
        {
            ClearOptions();
            optionGroup.gameObject.SetActive(true);

            foreach (EventOption option in options)
            {
                GameObject buttonObj = Instantiate(optionButtonPrefab.gameObject, optionGroup);
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                Button button = buttonObj.GetComponent<Button>();
                if (buttonText != null)
                {
                    buttonText.SetText(option.Text);
                }

                EventOption capturedOption = option; // クロージャ対策
                button.onClick.AddListener(() => onOptionSelected(capturedOption));
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
            if (nextTcs != null)
            {
                nextTcs?.TrySetResult();
                return;
            }

            OnEventConfirmed?.Invoke(currentEvent);
            Hide();
        }

        private void HandleCancelClicked()
        {
            OnEventCancelled?.Invoke();
            Hide();
        }

        public void ClearOptions()
        {
            foreach (Transform child in optionGroup)
            {
                Destroy(child.gameObject);
            }

            optionGroup.gameObject.SetActive(false);
        }

        private void HideAll()
        {
            base.Hide();
            eventTitleText.enabled = false;
            eventSpeakerText.enabled = false;
            eventMessageText.enabled = false;
        }
    }
}