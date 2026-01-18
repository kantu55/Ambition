
using Ambition.Utils;
using System;

namespace Ambition.Data.Master
{
    /// <summary>
    /// 物件データの定義（CSV読み込み用）
    /// </summary>
    [Serializable]
    public class HousingModel : IDataModel
    {
        private int id;
        private string houseName;
        private int houseGrade;
        private int monthlyRent;

        public int Id => id;
        public string HouseName => houseName;
        public int HouseGrade => houseGrade;
        public int MonthlyRent => monthlyRent;

        public void Initialize(CsvData data, int rowIndex)
        {
            id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "ID"));
            houseName = data.GetValue(rowIndex, "Name");
            houseGrade = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Grade"));
            monthlyRent = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "Rent"));
        }
    }
}
