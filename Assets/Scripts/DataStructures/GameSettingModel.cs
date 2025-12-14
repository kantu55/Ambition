using UnityEngine;
using UnityEditor;
using Ambition.Utility;

namespace Ambition.DataStructures
{
    /// <summary>
    /// ゲーム内の定数や設定値を定義するモデル
    /// Key-Value形式で数値を管理
    /// </summary>
	public class GameSettingModel : IDataModel
	{
        private string key;
        private float value;
        private string description;

        public int Id => 0;
        public string Key => key;
        public float Value => value;
        public string Description => description;

        public void Initialize(CsvData data, int rowIndex)
        {
            key = data.GetValue(rowIndex, "Key");
            value = CsvHelper.ConvertToFloat(data.GetValue(rowIndex, "Value"));
            description = data.GetValue(rowIndex, "Description");
        }
    }
}