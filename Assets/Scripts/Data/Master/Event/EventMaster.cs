using Ambition.Utility;
using System;

namespace Ambition.Data.Master.Event
{
    /// <summary>
    /// イベントの起点のみを管理
    /// </summary>
    [System.Serializable]
    public class EventMaster : IDataModel
    {
        public int Id { get; private set; }
        public int EventId { get; private set; }
        public string Title { get; private set; }
        public int EventType { get; private set; }
        public int FirstDialogGroupId { get; private set; }

        public void Initialize(CsvData data, int rowIndex)
        {
            EventId = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "EventId"));
            Title = data.GetValue(rowIndex, "Title");
            EventType = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "EventType"));
            FirstDialogGroupId = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "FirstDialogGroupId"));
        }
    }
}
