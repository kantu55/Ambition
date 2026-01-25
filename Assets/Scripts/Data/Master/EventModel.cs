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
        private int id; 
        private string title; 
        private string body;
        public int Id => id;
        public string Title => title;
        public string Body => body;

        public void Initialize(CsvData data, int rowIndex)
        {
            id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "id"));
            title = data.GetValue(rowIndex, "title");
            body = data.GetValue(rowIndex, "body");
        }
    }
}