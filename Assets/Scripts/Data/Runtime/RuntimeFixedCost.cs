using Ambition.Core.Config;
using Ambition.Core.Constants;
using Ambition.Data.Master;
using System;

namespace Ambition.Data.Runtime
{
    [Serializable]
	public class RuntimeFixedCost
	{
        public int Rent { get; private set; }
        public int Tax { get; private set; }
        public int Insurance { get; private set; }
        public int Maintenance { get; private set; }
        public int FoodCost { get; private set; }

        public int TotalCost => Rent + Tax + Insurance + Maintenance + FoodCost;

        public RuntimeFixedCost()
        {
            this.Rent = 0;
            this.Tax = 0;
            this.Insurance = 0;
            this.Maintenance = 0;
            this.FoodCost = 0;
        }

        public RuntimeFixedCost(FixedCostSaveData saveData)
        {
            this.Rent = saveData.Rent;
            this.Tax = saveData.Tax;
            this.Insurance = saveData.Insurance;
            this.Maintenance = saveData.Maintenance;
            this.FoodCost = saveData.FoodCost;
        }

        public FixedCostSaveData ToSaveData()
        {
            return new FixedCostSaveData
            {
                Rent = this.Rent,
                Tax = this.Tax,
                Insurance = this.Insurance,
                Maintenance = this.Maintenance,
                FoodCost = this.FoodCost,
            };
        }

        /// <summary>
        /// 家賃を更新
        /// </summary>
        public void UpdateRent(int newRent)
        {
            Rent = newRent;
        }

        /// <summary>
        /// 税金を更新（契約更改時など）
        /// 年俸の30%を12分割
        /// </summary>
        /// <param name="annualSalary">年俸</param>
        public void UpdateTax(int annualSalary)
        {
            float taxRate = GameSettings.GetFloat(SettingKeys.TaxRate, 0.3f);
            int estimatedAnnualTax = (int)(annualSalary * taxRate);
            Tax = estimatedAnnualTax / 12;
        }

        /// <summary>
        /// 保険料を更新
        /// </summary>
        public void UpdateInsurance(int planCost)
        {
            this.Insurance = planCost;
        }

        /// <summary>
        /// 維持費を再計算（設備レベルや所有物に基づく）
        /// </summary>
        public void RecalculateMaintenance(RuntimeEnvironmentStatus environmentStatus)
        {
            int cost = 0;

            // CSVから単価を取得
            int costGym = GameSettings.GetInt(SettingKeys.CostMaintGym, 5000);

            // 設備のレベルに応じて維持費がかかる設定
            cost += environmentStatus.GymLevel * costGym;

            // memo:車の維持費などをここに追加可能

            this.Maintenance = cost;
        }

        /// <summary>
        /// 食事ランクに基づいて食費を再計算
        /// </summary>
        /// <param name="rank">現在の食事ランク</param>
        public void RecalculateFoodCost(int rank)
        {
            // 動的にキーを生成 (例: Cost_Food_Lv1)
            string key = $"{SettingKeys.CostFoodPrefix}{rank}";

            // デフォルト値は 1万 (Lv0相当)
            int cost = GameSettings.GetInt(key, 10000);

            this.FoodCost = cost;
        }
    }
}
