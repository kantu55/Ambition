using Ambition.Utility;
using System;

namespace Ambition.Data.Master
{
    [Serializable]
	public class MatchTierDeltaModel : IDataModel
	{
        public int Id { get; private set; }

        public int Tier { get; private set; }
        public int DeltaT { get; private set; }
        public int DeltaCp { get; private set; }

        public void Initialize(CsvData data, int rowIndex)
        {
            Id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Id"));
            Tier = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Tier"));
            DeltaT = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "DeltaT"));
            DeltaCp = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "DeltaCp"));
        }
    }
}