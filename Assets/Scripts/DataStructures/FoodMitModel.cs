using Ambition.Utility;
using System;

namespace Ambition.DataStructures
{
    /// <summary>
    /// 食事ステータス定義
    /// </summary>
    [Serializable]
    public class FoodMitModel : IDataModel
    {
        private int id;
        private string tier;

        private string menuId;

        private string menuName;
        private string menuType;
        private int mitigHP;
        private int mitigMP;
        private int mitigCOND;
        private string notes;
        public int Id => id;
        public string Tier => tier;
        public string MenuId => menuId;
        public string MenuName => menuName;
        public string MenuType => menuType;
        public int MitigHP => mitigHP;
        public int MitigMP => mitigMP;
        public int MitigCOND => mitigCOND;
        public string Notes => notes;

        public void Initialize(CsvData data, int rowIndex)
        {
            id = data.GetValueToInt(rowIndex, "id");
            tier = data.GetValue(rowIndex, "tier");
            menuId = data.GetValue(rowIndex, "menu_id");
            menuName = data.GetValue(rowIndex, "menu_name");
            menuType = data.GetValue(rowIndex, "menu_type");
            mitigHP = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "mitig_HP"));
            mitigMP = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "mitig_MP"));
            mitigCOND = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "mitig_COND"));
            notes = data.GetValue(rowIndex, "notes");
        }
    }
}
