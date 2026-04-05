using Ambition.Core.Managers;
using Ambition.Data.Master;
using Ambition.Data.Runtime;
using Ambition.GameCore;
using System.Collections.Generic;
using UnityEngine;

namespace Ambition.UI.Equipment
{
    public class EquipmentShopFlowController : MonoBehaviour
    {
        [SerializeField] private EquipmentListPanel listPanel;
        [SerializeField] private EquipmentConfirmPanel confirmPanel;

        /// <summary>
        /// 今のゲーム状態参照（MainGameControllerから渡す）
        /// </summary>
        private RuntimeHouseholdBudget budget;
        private RuntimeEnvironmentStatus environment;

        private List<EquipmentModel> cachedList;

        private void Awake()
        {
            if (listPanel != null)
            {
                listPanel.OnEquipmentSelected += OnSelected;
                listPanel.OnClosePressed += Close;
                listPanel.Hide();
            }
            if (confirmPanel != null)
            {
                confirmPanel.OnConfirmPressed += OnConfirm;
                confirmPanel.OnCancelPressed += OnCancelConfirm;
            }

            HideAll();
        }

        private void OnDestroy()
        {
            if (listPanel != null)
            {
                listPanel.OnEquipmentSelected -= OnSelected;
                listPanel.OnClosePressed -= Close;
            }
            if (confirmPanel != null)
            {
                confirmPanel.OnConfirmPressed -= OnConfirm;
                confirmPanel.OnCancelPressed -= OnCancelConfirm;
            }
        }

        public void SetContext(RuntimeHouseholdBudget budget, RuntimeEnvironmentStatus environment)
        {
            this.budget = budget;
            this.environment = environment;
        }

        public void Open()
        {
            cachedList = DataManager.Instance.GetDatas<EquipmentModel>();
            if (cachedList == null) cachedList = new List<EquipmentModel>();

            if (listPanel != null) listPanel.Show(cachedList);
            if (confirmPanel != null) confirmPanel.Hide();
        }

        public void Close()
        {
            HideAll();
        }

        private void HideAll()
        {
            if (listPanel != null) listPanel.Hide();
            if (confirmPanel != null) confirmPanel.Hide();
        }

        private void OnSelected(EquipmentModel model)
        {
            if (confirmPanel != null) confirmPanel.Show(model);
        }

        private void OnCancelConfirm()
        {
            if (confirmPanel != null) confirmPanel.Hide();
        }

        private void OnConfirm(EquipmentModel model)
        {
            if (budget == null || environment == null)
            {
                Debug.LogWarning("[EquipmentShopFlowController] context is null");
                return;
            }

            if (EquipmentPurchaseService.TryPurchase(budget, environment, model, out var reason))
            {
                Debug.Log($"設備購入: {model.Name}");
                // 購入後は閉じる（必要なら継続表示でもOK）
                Close();
            }
            else
            {
                Debug.LogWarning($"設備購入できません: {reason}");
                // ここはトースト/ダイアログ等に差し替え
            }
        }
    }
}