using Ambition.Utility;
using System;

namespace Ambition.Data.Master
{
    /// <summary>
    /// 家計の初期ステータス定義
    /// </summary>
    [Serializable]
	public class BudgetModel : IDataModel
	{
        private int id;
        private long initialMoney;

        public int Id => id;
        public long InitialMoney => initialMoney;

        public void Initialize(CsvData data, int rowIndex)
        {
            id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "ID"));
            initialMoney = CsvHelper.ConvertToLong(data.GetValue(rowIndex, "Money"));
        }

    }
}