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
        private int health;
        private int mental;
        private int fatigue;
        private int muscle;
        private int technique;
        private int concentration;
        private string evaluation;

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
        /// ヘルス（体力）
        /// </summary>
        public int Health => health;

        /// <summary>
        /// メンタル
        /// </summary>
        public int Mental => mental;

        /// <summary>
        /// 疲労累積値
        /// </summary>
        public int Fatigue => fatigue;

        /// <summary>
        /// 能力値：筋力
        /// </summary>
        public int Muscle => muscle;

        /// <summary>
        /// 能力値：技術
        /// </summary>
        public int Technique => technique;

        /// <summary>
        /// 能力値：集中
        /// </summary>
        public int Concentration => concentration;

        /// <summary>
        /// 選手の総合評価
        /// </summary>
        public string Evaluation => evaluation;

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
            string healthString = data.GetValue(rowIndex, "Health");
            string mentalString = data.GetValue(rowIndex, "Mental");
            string fatigueString = data.GetValue(rowIndex, "Fatigue");
            string muscleString = data.GetValue(rowIndex, "Muscle");
            string techniqueString = data.GetValue(rowIndex, "Technique");
            string concentrationString = data.GetValue(rowIndex, "Concentration");
            string evaluationString = data.GetValue(rowIndex, "Evaluation");

            // 型変換
            this.id = CsvHelper.ConvertToInt(idString);
            this.name = nameString;
            this.age = CsvHelper.ConvertToInt(ageString, defaultValue: 0);
            this.health = CsvHelper.ConvertToInt(healthString, defaultValue: 100);
            this.mental = CsvHelper.ConvertToInt(mentalString, defaultValue: 50);
            this.fatigue = CsvHelper.ConvertToInt(fatigueString, defaultValue: 0);
            this.muscle = CsvHelper.ConvertToInt(muscleString, defaultValue: 0);
            this.technique = CsvHelper.ConvertToInt(techniqueString, defaultValue: 0);
            this.concentration = CsvHelper.ConvertToInt(concentrationString, defaultValue: 0);
            this.evaluation = evaluationString;
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