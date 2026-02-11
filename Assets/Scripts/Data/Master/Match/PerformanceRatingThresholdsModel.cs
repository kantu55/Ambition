using Ambition.Utility;
using System;

namespace Ambition.Data.Master
{
    [Serializable]
	public class PerformanceRatingThresholdsModel : IDataModel
	{
        public int Id { get; private set; }
        public int Tier { get; private set; }
        public int RatingAtLeast { get; private set; }

        public void Initialize(CsvData data, int rowIndex)
        {
            Id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Id"));
            Tier = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Tier"));
            RatingAtLeast = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "RatingAtLeast"));
        }
    }
}