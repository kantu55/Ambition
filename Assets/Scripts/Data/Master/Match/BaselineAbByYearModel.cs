using Ambition.Utility;
using System;

namespace Ambition.Data.Master
{
    [Serializable]
	public class BaselineAbByYearModel : IDataModel
	{
        public int Id { get; private set; }

        public int Year { get; private set; }
        public int BaselineAb { get; private set; }

        public void Initialize(CsvData data, int rowIndex)
        {
            Id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Id"));
            Year = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Year"));
            BaselineAb = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "BaselineAb"));
        }
    }
}