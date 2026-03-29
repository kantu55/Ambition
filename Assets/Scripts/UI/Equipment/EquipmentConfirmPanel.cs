using Ambition.Data.Master;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI.Equipment
{
    public class EquipmentConfirmPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;

        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI equipmentNameText;

        [SerializeField] private TextMeshProUGUI purchasePriceText;
        [SerializeField] private TextMeshProUGUI monthlyCostText;
        [SerializeField] private TextMeshProUGUI durationText;

        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        public event Action<EquipmentModel> OnConfirmPressed;
        public event Action OnCancelPressed;

        private EquipmentModel current;

        private void Awake()
        {
            if (confirmButton != null) confirmButton.onClick.AddListener(HandleConfirm);
            if (cancelButton != null) cancelButton.onClick.AddListener(HandleCancel);

            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveListener(HandleConfirm);
            if (cancelButton != null) cancelButton.onClick.RemoveListener(HandleCancel);
        }

        public void Show(EquipmentModel model)
        {
            current = model;
            if (panelRoot != null) panelRoot.SetActive(true);

            if (titleText != null) titleText.text = "購入しますか？";
            if (equipmentNameText != null) equipmentNameText.text = model != null ? model.Name : "";

            if (purchasePriceText != null) purchasePriceText.text = $"購入金額: ¥{model.PurchaseCostYen:N0}";
            if (monthlyCostText != null) monthlyCostText.text = $"月額費用: ¥{model.MonthlyCostYen:N0} / 月";
            if (durationText != null) durationText.text = $"使用期間: {model.DurabilityMonths}ヶ月";
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            current = null;
        }

        private void HandleConfirm()
        {
            if (current == null) return;
            OnConfirmPressed?.Invoke(current);
        }

        private void HandleCancel()
        {
            OnCancelPressed?.Invoke();
        }
    }
}