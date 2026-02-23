using Ambition.Utility;
using System;

namespace Ambition.Data.Master.Event
{
    /// <summary>
    /// 1行＝1ページとして扱い、ページ順に読み込む
    /// </summary>
    [System.Serializable]
    public class EventDialog : IDataModel
    {
        public int Id { get; private set; }
        public int DialogGroupId { get; private set; }
        public int PageNumber { get; private set; }
        public int SpeakerId { get; private set; }
        public string Text { get; private set; }
        public int OptionGroupId { get; private set; }
        public int NextEventId { get; private set; }

        public void Initialize(CsvData data, int rowIndex)
        {
            Id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Id"));
            DialogGroupId = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "DialogGroupId"));
            PageNumber = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "PageNumber"));
            SpeakerId = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "SpeakerId"));
            Text = data.GetValue(rowIndex, "Text");
            OptionGroupId = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "OptionGroupId"));
            NextEventId = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "NextEventId"));
        }
    }
}
