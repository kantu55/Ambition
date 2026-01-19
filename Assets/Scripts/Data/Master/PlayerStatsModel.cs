using UnityEngine;
using Ambition.Utility;

namespace Ambition.Data.Master
{
    /// <summary>
    /// プロ野球選手（夫）の基礎能力値と情報を格納するデータモデル
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
        private int ability;
        private int condition;
        private int love;
        private int teamEvaluation;
        private int salary;

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
        /// 選手能力
        /// </summary>
        public int Ability => ability;

        /// <summary>
        /// 調子
        /// </summary>
        public int Condition => condition;

        /// <summary>
        /// 選手の総合評価
        /// </summary>
        public int TeamEvaluation => teamEvaluation;

        /// <summary>
        /// 年棒
        /// </summary>
        public int Salary => salary;

        /// <summary>
        /// 夫婦仲（愛情度）
        /// </summary>
        public int Love => love;

        /// <summary>
        /// 現在の状態をセーブデータ構造体に変換して返します。
        /// </summary>
        public PlayerSaveData ToSaveData()
        {
            return new PlayerSaveData
            {
                Id = this.id,
                Name = this.name,
                PositionString = this.position.ToString(),
                Age = this.age,
                Health = this.health,
                Mental = this.mental,
                Condition = this.condition,
                Ability = this.ability,
                TeamEvaluation = this.teamEvaluation,
                Salary = this.salary,
                Love = this.love
            };
        }

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
            string conditionString = data.GetValue(rowIndex, "Condition");
            string abilityString = data.GetValue(rowIndex, "Ability");
            string teamEvaluationString = data.GetValue(rowIndex, "TeamEvaluation");
            string salaryString = data.GetValue(rowIndex, "Salary");
            string loveString = data.GetValue(rowIndex, "Love");

            // 型変換
            this.id = CsvHelper.ConvertToInt(idString);
            this.name = nameString;
            this.age = CsvHelper.ConvertToInt(ageString, defaultValue: 25);
            this.health = CsvHelper.ConvertToInt(healthString, defaultValue: 100);
            this.mental = CsvHelper.ConvertToInt(mentalString, defaultValue: 50);
            this.condition = CsvHelper.ConvertToInt(conditionString, defaultValue: 50);
            this.ability = CsvHelper.ConvertToInt(abilityString, defaultValue: 0);
            this.teamEvaluation = CsvHelper.ConvertToInt(teamEvaluationString, defaultValue: 0);
            this.position = ParsePosition(positionString);
            this.salary = CsvHelper.ConvertToInt(salaryString, defaultValue: 2400000);
            this.love = CsvHelper.ConvertToInt(loveString, defaultValue: 100);
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