using Ambition.Core.Managers;
using Ambition.Data.Master;
using Ambition.Data.Runtime;
using System.Linq;
using UnityEngine;

namespace Ambition.GameCore
{
    public static class EquipmentPurchaseService
    {
        /// <summary>
        /// 仕様フラグ：
        /// trueにすると「同カテゴリでは、IDがより大きい設備しか買えない」制約を適用
        /// </summary>
        public static bool OnlyHigherIdInSameCategory = false;

        public static bool CanPurchase(RuntimeHouseholdBudget budget, RuntimeEnvironmentStatus environment, EquipmentModel equipment, out string reason)
        {
            reason = "";

            if (budget == null || environment == null || equipment == null)
            {
                reason = "データ不正";
                return false;
            }

            // 重複購入不可
            if (environment.OwnedEquipments != null && environment.OwnedEquipments.Any(v => v.EquipId == equipment.Id))
            {
                reason = "既に購入済みです";
                return false;
            }

            // カテゴリ制約（将来仕様）
            if (OnlyHigherIdInSameCategory && environment.OwnedEquipments != null)
            {
                // 同カテゴリの所持設備の中で最大ID
                var masters = DataManager.Instance.GetDatas<EquipmentModel>();
                int ownedMaxId = 0;

                foreach (var owned in environment.OwnedEquipments)
                {
                    var m = masters.FirstOrDefault(v => v.Id == owned.EquipId);
                    if (m != null && m.Category == equipment.Category)
                    {
                        ownedMaxId = Mathf.Max(ownedMaxId, m.Id);
                    }
                }

                if (ownedMaxId > 0 && equipment.Id <= ownedMaxId)
                {
                    reason = "同カテゴリは上位設備のみ購入できます";
                    return false;
                }
            }

            // 所持金チェック
            if ((ulong)budget.CurrentSavings < equipment.PurchaseCostYen)
            {
                reason = "資金不足です";
                return false;
            }

            return true;
        }

        public static bool TryPurchase(RuntimeHouseholdBudget budget, RuntimeEnvironmentStatus environment, EquipmentModel equipment, out string reason)
        {
            if (!CanPurchase(budget, environment, equipment, out reason))
            {
                return false;
            }

            if (!budget.TrySpend((int)equipment.PurchaseCostYen))
            {
                reason = "資金不足です";
                return false;
            }

            environment.AddEquipment(equipment);

            reason = "";
            return true;
        }
    }
}
