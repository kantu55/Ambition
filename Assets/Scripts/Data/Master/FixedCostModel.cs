using UnityEngine;
using UnityEditor;
using Ambition.Utils;

namespace Ambition.Data.Master
{
	public class FixedCostModel : IDataModel
	{
        private int id;
        private int rent;
        private int tax;
        private int insurance;
        private int maintenance;
        public int Id => id;
        public int Rent => rent;
        public int Tax => tax;
        public int Insurance => insurance;
        public int Maintenance => maintenance;

        public void Initialize(CsvData data, int rowIndex)
        {
            id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "ID"));
            tax = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Tax"));
            rent = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Rent"));
            insurance = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Insurance"));
            maintenance = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Maintenance"));
        }
    }
}