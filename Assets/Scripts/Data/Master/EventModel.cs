using Ambition.Utility;
using System;

namespace Ambition.Data.Master
{ 
    /// <summary>
    /// ランダムイベントのデータモデル
    /// </summary> 
    [Serializable] 
    public class EventModel : IDataModel 
    {
        public int Id { get; private set; }
        public string EventId { get; private set; }
        public string Title { get; private set; }
        public string Tags { get; private set; }
        public int Weight { get; private set; }
        public string Conditions { get; private set; }
        public string Effect { get; private set; }
        public string Note { get; private set; }

        public void Initialize(CsvData data, int rowIndex)
        {
            Id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "id"));
            EventId = data.GetValue(rowIndex, "event_id");
            Title = data.GetValue(rowIndex, "title");
            Tags = data.GetValue(rowIndex, "tags");
            Weight = data.GetValueToInt(rowIndex, "weight");
            Conditions = data.GetValue(rowIndex, "cond");
            Effect = data.GetValue(rowIndex, "effect");
            Note = data.GetValue(rowIndex, "note");
        }
    }
}
