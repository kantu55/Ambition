using Ambition.Data.Master;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI.Equipment
{
    public class EquipmentListPanel : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button itemButtonPrefab;
        [SerializeField] private Button closeButton;

        public event Action<EquipmentModel> OnEquipmentSelected;
        public event Action OnClosePressed;

        private readonly List<Button> instantiated = new List<Button>();

        private void Awake()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => OnClosePressed?.Invoke());
                closeButton.gameObject.SetActive(false);
            }

            if (backgroundImage != null)
            {
                backgroundImage.enabled = false;
            }

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(() => OnClosePressed?.Invoke());
            }
        }

        public void Show(List<EquipmentModel> equipments)
        {
            gameObject.SetActive(true);
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(true);
            }

            if (backgroundImage != null)
            {
                backgroundImage.enabled = true;
            }

            Clear();

            if (equipments == null)
            {
                return;
            }

            foreach (var e in equipments)
            {
                var btn = Instantiate(itemButtonPrefab, panelRoot.transform);
                instantiated.Add(btn);

                var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{e.Name} (¥{e.PurchaseCostYen:N0})";
                }

                btn.onClick.AddListener(() => OnEquipmentSelected?.Invoke(e));
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(false);
            }

            if (backgroundImage != null)
            {
                backgroundImage.enabled = false;
            }

            Clear();
        }

        private void Clear()
        {
            for (int i = 0; i < instantiated.Count; i++)
            {
                if (instantiated[i] != null)
                {
                    Destroy(instantiated[i].gameObject);
                }
            }

            instantiated.Clear();
        }
    }
}