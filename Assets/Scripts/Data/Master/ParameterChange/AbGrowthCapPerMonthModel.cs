using Ambition.Utility;
using System;

namespace Ambition.Data.Master
{
    [Serializable]
    public class AbGrowthCapPerMonth : IDataModel
    {
        public int Id { get; private set; }
        public float Cap { get; private set; }
        public void Initialize(CsvData data, int rowIndex)
        {
            Id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Id"));
            Cap = CsvHelper.ConvertToFloat(data.GetValue(rowIndex, "Cap"));
        }
    }
}
