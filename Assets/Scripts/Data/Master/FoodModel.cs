using Ambition.Utils;
using System;
using UnityEngine.Tilemaps;

namespace Ambition.Data.Master
{
    /// <summary>
    /// 食事ステータス定義
    /// </summary>
    [Serializable]
    public class FoodModel : IDataModel
    {
        // --- メンバ変数 ---
        private int kind;
        private int id;
        private string tier;
        private string name;
        private int primary;
        private int type;
        private int monthlyCost;
        private int costLimit;
        private int mitigTotal;
        private string notes;

        // --- プロパティ ---
        public int Kind => kind;
        public string Tier => tier;
        public int Id => id;
        public string Name => name;
        public int Primary => primary;
        public int Type => type;
        public int MonthlyCost => monthlyCost;
        public int CostLimit => costLimit;
        public int MitigTotal => mitigTotal;
        public string Notes => notes;
        public void Initialize(CsvData data, int rowIndex)
        {
            kind = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "kind"));
            id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "id"));
            tier = data.GetValue(rowIndex, "tier");
            name = data.GetValue(rowIndex, "name");
            primary = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "primary"));
            type = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "type"));
            monthlyCost = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "monthly_cost"));
            costLimit = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "cost_limit"));
            mitigTotal = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "mitig_total"));
            notes = data.GetValue(rowIndex, "notes");
        }
    }
}
