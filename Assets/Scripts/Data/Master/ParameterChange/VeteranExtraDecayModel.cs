using Ambition.Utility;
using System;

namespace Ambition.Data.Master
{
    [Serializable]
	public class VeteranExtraDecayModel : IDataModel
	{
        public int Id { get; private set; }

        public int StatType { get; private set; }
        public float Value { get; private set; }

        public void Initialize(CsvData data, int rowIndex)
        {
            Id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Id"));
            StatType = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "StatType"));
            Value = CsvHelper.ConvertToFloat(data.GetValue(rowIndex, "Value"));
        }
    }
}