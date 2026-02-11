using Ambition.Utility;
using System;

namespace Ambition.Data.Master
{
    [Serializable]
    public class AbGrowthMultByCondStageModel : IDataModel
    {
        public int Id { get; private set; }
        public int CondStage { get; private set; }
        public float GrowthMulti { get; private set; }
        public void Initialize(CsvData data, int rowIndex)
        {
            Id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Id"));
            CondStage = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "CondStage"));
            GrowthMulti = CsvHelper.ConvertToFloat(data.GetValue(rowIndex, "GrowthMulti"));
        }
    }
}
