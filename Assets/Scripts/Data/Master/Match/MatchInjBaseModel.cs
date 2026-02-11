using Ambition.Utility;
using System;

namespace Ambition.Data.Master
{
    [Serializable]
	public class MatchInjBaseModel : IDataModel
	{
        public int Id { get; private set; }
        public int InjType { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }
        public float Value { get; private set; }

        public void Initialize(CsvData data, int rowIndex)
        {
            Id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Id"));
            InjType = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "InjType"));
            Min = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Min"));
            Max = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Max"));
            Value = CsvHelper.ConvertToFloat(data.GetValue(rowIndex, "Value"));
        }
    }
}