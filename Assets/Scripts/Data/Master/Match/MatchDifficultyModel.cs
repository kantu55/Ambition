using Ambition.Utility;
using System;

namespace Ambition.Data.Master
{
    [Serializable]
	public class MatchDifficultyModel : IDataModel
	{
        public int Id { get; private set; }

        public int DifficultyType { get; private set; }
        public float ConditionCoefficient { get; private set; }
        public float MpCoefficient { get; private set; }

        public void Initialize(CsvData data, int rowIndex)
        {
            Id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Id"));
            DifficultyType = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "DifficultyType"));
            ConditionCoefficient = CsvHelper.ConvertToFloat(data.GetValue(rowIndex, "CondCoef"));
            MpCoefficient = CsvHelper.ConvertToFloat(data.GetValue(rowIndex, "MpCoef"));
        }
    }
}