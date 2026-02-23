using Ambition.Utility;
using System;

namespace Ambition.Data.Master.Event
{
    /// <summary>
    /// 選択肢をテキストや選択された場合のパラメータ変動値を管理
    /// </summary>
    [System.Serializable]
    public class EventOption : IDataModel
    {
        public int Id { get; private set; }
        public int OptionGroupId { get; private set; }
        public bool IsAutoApply { get; private set; }
        public string Text { get; private set; }
        public int NextDialogGroupId { get; private set; }
        public int CostMoney { get; private set; }
        public int MitigHp { get; private set; }
        public int MitigMp { get; private set; }
        public int MitigCond { get; private set; }

        public void Initialize(CsvData data, int rowIndex)
        {
            Id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Id"));
            OptionGroupId = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "OptionGroupId"));
            IsAutoApply = CsvHelper.ConvertToBool(data.GetValue(rowIndex, "IsAutoApply"));
            Text = data.GetValue(rowIndex, "Text");
            NextDialogGroupId = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "NextDialogGroupId"));
            CostMoney = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "CostMoney"));
            MitigHp = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "MitigHp"));
            MitigMp = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "MitigMp"));
            MitigCond = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "MitigCond"));
        }
    }
}
