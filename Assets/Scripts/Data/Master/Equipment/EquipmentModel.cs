using Ambition.Utility;
using System;

namespace Ambition.Data.Master
{
    public class EquipmentModel : IDataModel
    {
        public int Id { get; private set; }
        public int EquipId { get; private set; }
        public string Name { get; private set; }
        public int Category { get; private set; }
        public ulong PurchaseCostYen { get; private set; }
        public ulong MonthlyCostYen { get; private set; }
        public int DurabilityMonths { get; private set; }
        public int Req { get; private set; }

        public void Initialize(CsvData data, int rowIndex)
        {
            EquipId = data.GetValueToInt(rowIndex, "EquipId");
            Name = data.GetValue(rowIndex, "Name");
            Category = data.GetValueToInt(rowIndex, "Category");
            PurchaseCostYen = data.GetValueToUlong(rowIndex, "PurchaseCostYen");
            MonthlyCostYen = data.GetValueToUlong(rowIndex, "MonthlyCostYen");
            DurabilityMonths = data.GetValueToInt(rowIndex, "DurabilityMonths");
            Req = data.GetValueToInt(rowIndex, "Req");
        }
    }
}
