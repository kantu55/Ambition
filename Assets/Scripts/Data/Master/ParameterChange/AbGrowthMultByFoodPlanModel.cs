using Ambition.Utility;
using System;

namespace Ambition.Data.Master
{
    [Serializable]
    public class AbGrowthMultByFoodPlanModel : IDataModel
    {
        public int Id { get; private set; }
        public int FoodTier { get; private set; }
        public float GrowthMulti { get; private set; }
        public void Initialize(CsvData data, int rowIndex)
        {
            Id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Id"));
            FoodTier = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "FoodTier"));
            GrowthMulti = CsvHelper.ConvertToFloat(data.GetValue(rowIndex, "GrowthMulti"));
        }
    }
}
