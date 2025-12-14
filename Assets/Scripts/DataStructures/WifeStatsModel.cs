
using System;
using Ambition.Utility;

namespace Ambition.DataStructures
{
    /// <summary>
    /// 妻の初期ステータス定義
    /// </summary>
    [Serializable]
    public class WifeStatsModel : IDataModel
    {
        private int id;
        private int initialHealth;
        private int initialCooking;
        private int initialLooks;
        private int initialSocial;

        public int Id => id;
        public int InitialHealth => initialHealth;
        public int InitialCooking => initialCooking;
        public int InitialLooks => initialLooks;
        public int InitialSocial => initialSocial;

        public void Initialize(CsvData data, int rowIndex)
        {
            id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "ID"));
            initialHealth = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Health"), 100);
            initialCooking = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Cooking"), 1);
            initialLooks = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Looks"), 1);
            initialSocial = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Social"), 0);
        }
    }
}