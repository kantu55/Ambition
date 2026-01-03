
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
        private int initialCooking;
        private int initialCare;
        private int initialPR;
        private int initialCoach;

        public int Id => id;
        public int InitialCooking => initialCooking;
        public int InitialCare => initialCare;
        public int InitialPR => initialPR;
        public int InitialCoach => initialCoach;

        public void Initialize(CsvData data, int rowIndex)
        {
            id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "id"));
            initialCooking = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "cooking"));
            initialCare = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "care"));
            initialPR = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "PR"));
            initialCoach = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "coach"));
        }
    }
}
