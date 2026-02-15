using Ambition.Utility;
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
        public int Id { get; private set; }
        public int TierType { get; private set; }
        public string TierName { get; private set; }
        public int Price { get; private set; }
        public string MenuName { get; private set; }
        public int MenuType { get; private set; }
        public int MitigHP { get; private set; }
        public int MitigMP { get; private set; }
        public int MitigCOND { get; private set; }
        public string Notes { get; private set; }
        public void Initialize(CsvData data, int rowIndex)
        {
            Id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Id"));
            TierType = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "TierType"));
            TierName = CsvHelper.SanitizeString(data.GetValue(rowIndex, "TierName"));
            Price = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Price"));
            MenuType = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "MenuType"));
            MenuName = CsvHelper.SanitizeString(data.GetValue(rowIndex, "MenuName"));
            MitigHP = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "MitigHP"));
            MitigMP = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "MitigMP"));
            MitigCOND = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "MitigCOND"));
            Notes = data.GetValue(rowIndex, "Note");
        }
    }
}
