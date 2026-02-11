using Ambition.Utility;
using System;

namespace Ambition.Data.Master
{
    [Serializable]
    public class AbBaseGrowthPerMonthModel : IDataModel
    {
        public int Id { get; private set; }
        public int PlayerCareerStage { get; private set; }
        public float Value { get; private set; }
        public void Initialize(CsvData data, int rowIndex)
        {
            Id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Id"));
            PlayerCareerStage = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "PlayerCareerStage"));
            Value = CsvHelper.ConvertToFloat(data.GetValue(rowIndex, "Value"));
        }
    }
}
