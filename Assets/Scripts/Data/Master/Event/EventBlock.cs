using Ambition.Utility;
using System;

namespace Ambition.Data.Master.Event
{
    [System.Serializable]
    public class EventBlock : IDataModel
    {
        public int Id { get; private set; }
        public int Year { get; private set; }
        public int PositiveCount { get; private set; }
        public int NegativeCount { get; private set; }

        public void Initialize(CsvData data, int rowIndex)
        {
            Year = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Year"));
            PositiveCount = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "PositiveCount"));
            NegativeCount = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "NegativeCount"));
        }
    }
}
