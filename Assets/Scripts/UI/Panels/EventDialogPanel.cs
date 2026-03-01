using Ambition.Core.Managers;
using Ambition.Data.Master;
using Ambition.Data.Master.Event;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
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
        private UniTaskCompletionSource _nextTcs;
        private UniTaskCompletionSource<EventOption> _optionTcs;

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
        /// イベントのダイアログフローを非同期で表示する。
        /// ダイアログを順番に表示し、選択肢がある場合は選択を待つ。
        /// </summary>
        public async UniTask ShowEventAsync(EventMaster eventData)
        {
            if (eventData == null) return;

            currentEvent = eventData;

            if (eventTitleText != null)
            {
                eventTitleText.SetText(eventData.Title);
            }

            Show();

            var allDialogs = DataManager.Instance.GetDatas<EventDialog>();
            var allOptions = DataManager.Instance.GetDatas<EventOption>();

            int currentGroupId = eventData.FirstDialogGroupId;

            while (currentGroupId > 0)
            {
                var dialogs = allDialogs
                    .Where(d => d.DialogGroupId == currentGroupId)
                    .OrderBy(d => d.PageNumber)
                    .ToList();

                if (dialogs.Count == 0) break;

                int nextGroupId = 0;

                foreach (var dialog in dialogs)
                {
                    ShowDialog(dialog);

                    if (dialog.OptionGroupId > 0)
                    {
                        var options = allOptions
                            .Where(o => o.OptionGroupId == dialog.OptionGroupId)
                            .ToList();

                        if (confirmButton != null) confirmButton.gameObject.SetActive(false);

                        _optionTcs = new UniTaskCompletionSource<EventOption>();
                        ShowOptions(options, opt =>
                        {
                            ClearOptions();
                            _optionTcs?.TrySetResult(opt);
                        });

                        var selectedOption = await _optionTcs.Task;
                        _optionTcs = null;

                        if (confirmButton != null) confirmButton.gameObject.SetActive(true);

                        nextGroupId = selectedOption.NextDialogGroupId;
                        // オプション選択でグループの遷移先が決まるため、残りのダイアログはスキップして次のグループへ進む
                        break;
                    }
                    else
                    {
                        if (confirmButton != null) confirmButton.gameObject.SetActive(true);

                        _nextTcs = new UniTaskCompletionSource();
                        await _nextTcs.Task;
                        _nextTcs = null;
                    }
                }

                currentGroupId = nextGroupId;
            }

            Hide();
            OnEventConfirmed?.Invoke(currentEvent);
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

        private string FormatChange(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        private void HandleConfirmClicked()
        {
            if (_nextTcs != null)
            {
                _nextTcs.TrySetResult();
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