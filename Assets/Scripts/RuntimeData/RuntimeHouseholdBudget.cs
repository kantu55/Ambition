using UnityEngine;
using UnityEditor;
using System;
using Ambition.DataStructures;

namespace Ambition.RuntimeData
{
    [Serializable]
    public class RuntimeHouseholdBudget
    {
        public long CurrentSavings { get; private set; }
        public RuntimeFixedCost FixedCost { get; private set; }

        public RuntimeHouseholdBudget(long initialMoney)
        {
            CurrentSavings = initialMoney;
            FixedCost = new RuntimeFixedCost();
        }

        public RuntimeHouseholdBudget(BudgetSaveData saveData)
        {
            CurrentSavings = saveData.CurrentSavings;
            FixedCost = new RuntimeFixedCost(saveData.FixedCostSaveData);
        }

        public BudgetSaveData ToSaveData()
        {
            return new BudgetSaveData
            {
                CurrentSavings = CurrentSavings,
                FixedCostSaveData = FixedCost.ToSaveData(),
            };
        }

        public void AddIncome(int amount)
        {
            CurrentSavings += amount;
        }

        public bool TrySpend(int amount)
        {
            if (CurrentSavings >= amount)
            {
                CurrentSavings -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 月次処理: 固定費を支払い
        /// </summary>
        public void PayMonthlyFixedCosts()
        {
            int total = FixedCost.TotalCost;
            if (TrySpend(total))
            {
                Debug.Log($"固定費を支払いました: 合計 {total:N0}円 " +
                          $"(家賃:{FixedCost.Rent}, 税金:{FixedCost.Tax}, " +
                          $"保険:{FixedCost.Insurance}, 維持費:{FixedCost.Maintenance})");
            }
            else
            {
                Debug.LogWarning("資金不足で固定費が払えません！");
                CurrentSavings = 0;
            }
        }
    }
}
