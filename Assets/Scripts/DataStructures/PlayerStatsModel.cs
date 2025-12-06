using UnityEngine;
using Ambition.Utility;
using Ambition.GameCore;

namespace Ambition.DataStructures
{
    /// <summary>
    /// プロ野球選手の基礎能力値と情報を格納するデータモデル
    /// </summary>
    public class PlayerStatsModel : IDataModel
    {
        /// <summary>
        /// 選手の守備位置
        /// </summary>
        public enum PositionType
        {
            UNKNOWN,
            PITCHER,
            CATCHER,
            INFIELD,
            OUTFIELD
        }

        // メンバ変数
        private int id;
        private string name;
        private PositionType position;
        private int age;
        private float power;
        private float stamina;
        private bool isRookie;

        // プロパティ

        /// <summary>
        /// 選手の固有ID。
        /// </summary>
        public int Id => id;

        /// <summary>
        /// 選手名
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 守備位置
        /// </summary>
        public PositionType Position => position;

        /// <summary>
        /// 年齢
        /// </summary>
        public int Age => age;

        /// <summary>
        /// パワー
        /// </summary>
        public float Power => power;

        /// <summary>
        /// スタミナ
        /// </summary>
        public float Stamina => stamina;

        /// <summary>
        /// ルーキーフラグ
        /// </summary>
        public bool IsRookie => isRookie;

        /// <summary>
        /// CSVデータからモデルを初期化
        /// </summary>
        public void Initialize(CsvData data, int rowIndex)
        {
            // 列名を使用して安全にデータを取得
            string idString = data.GetValue(rowIndex, "ID");
            string nameString = data.GetValue(rowIndex, "Name");
            string positionString = data.GetValue(rowIndex, "Position");
            string ageString = data.GetValue(rowIndex, "Age");
            string powerString = data.GetValue(rowIndex, "Power");
            string staminaString = data.GetValue(rowIndex, "Stamina");
            string isRookieString = data.GetValue(rowIndex, "IsRookie");

            // 型変換
            this.id = CsvHelper.ConvertToInt(idString);
            this.name = nameString;
            this.age = CsvHelper.ConvertToInt(ageString, defaultValue: 0);
            this.power = CsvHelper.ConvertToFloat(powerString);
            this.stamina = CsvHelper.ConvertToFloat(staminaString);
            this.isRookie = CsvHelper.ConvertToInt(isRookieString) == 1;
            this.position = ParsePosition(positionString);
        }

        /// <summary>
        /// 守備位置の文字列を列挙型に変換
        /// </summary>
        private static PositionType ParsePosition(string positionString)
        {
            string upperPosition = positionString.ToUpper();

            switch (upperPosition)
            {
                case "P": return PositionType.PITCHER;
                case "C": return PositionType.CATCHER;
                case "IF": return PositionType.INFIELD;
                case "OF": return PositionType.OUTFIELD;
                default:
                    Debug.LogWarning($"不明な守備位置: {positionString}");
                    return PositionType.UNKNOWN;
            }
        }
    }
}