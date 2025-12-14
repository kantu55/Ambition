using Ambition.Utility;
using System;

namespace Ambition.DataStructures
{
    [Serializable]
    public class WifeActionModel : IDataModel
    {
        /// <summary>
        /// アクションのメインカテゴリ（大項目）
        /// </summary>
        public enum ActionMainCategory
        {
            NONE = 0,
            SUPPORT_HUSBAND = 1,  // 夫を支える
            SELF_POLISH = 2,      // 自分を磨く
            ENVIRONMENT = 3,      // 環境を整える
            PR_SALES = 4          // 広報・営業
        }

        /// <summary>
        /// サブカテゴリ（中項目）
        /// </summary>
        public enum ActionSubCategory
        {
            NONE = 0,

            // --- 夫を支える ---
            COOKING = 101,             // 手料理
            MENTAL_CARE = 102,         // メンタル管理（励ます、相談など）
            BODY_MAINTENANCE = 103,    // ボディメンテナンス（マッサージなど）
            SCOLD = 104,               // 叱る（尻を叩く）

            // --- 自分を磨く ---
            HOUSEWORK_SKILL_UP = 202,    // 家事スキル上げ
            APPEARANCE_INVESTMENT = 203, // 容姿投資
            WIFE_REST = 204,             // 妻の休養

            // --- 3. 環境を整える (300番台) ---
            INVESTMENT = 301,          // 設備投資
            MOVE_BASE = 302,           // 拠点変更（引っ越し）
            GIFT_TO_HUSBAND = 303,     // 夫へのプレゼント

            // --- 4. 広報・営業 (400番台) ---
            PUBLIC_RELATIONS = 401,     // 広報
            BUZZ_CHARGE = 402,          // バズり課金
            EGOSEARCH_MONITORING = 403, // エゴサ/監視
            TEAM_ASSISTANCE = 404,      // チームのお手伝い
            REQUEST_SPONSOR = 405       // スポンサー要請
        }

        // --- メンバ変数 ---
        private int id;
        private string name;
        private ActionMainCategory mainCategory;
        private ActionSubCategory subCategory;
        private string subCategoryName01;
        private string subCategoryName02;
        private string description;

        // コスト
        private int costMoney;
        private int costWifeHealth;

        // 夫への効果
        private int healthChange;
        private int mentalChange;
        private int fatigueChange;
        private int loveChange;

        // 能力成長
        private int muscleChange;
        private int techniqueChange;
        private int concentrationChange;

        // 妻・その他への効果
        private int stressChange;
        private int skillExp;

        // --- プロパティ ---
        public int Id => id;
        public string Name => name;
        public ActionMainCategory MainCategory => mainCategory;
        public ActionSubCategory SubCategory => subCategory;
        public string SubCategoryName01 => subCategoryName01;
        public string SubCategoryName02 => subCategoryName02;
        public string Description => description;

        public int CostMoney => costMoney;
        public int CostWifeHealth => costWifeHealth;

        public int HealthChange => healthChange;
        public int MentalChange => mentalChange;
        public int FatigueChange => fatigueChange;
        public int LoveChange => loveChange;

        public int MuscleChange => muscleChange;
        public int TechniqueChange => techniqueChange;
        public int ConcentrationChange => concentrationChange;

        public int StressChange => stressChange;
        public int SkillExp => skillExp;

        public void Initialize(CsvData data, int rowIndex)
        {
            id = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "ID"));
            name = data.GetValue(rowIndex, "Name");

            mainCategory = (ActionMainCategory)CsvHelper.ConvertToInt(data.GetValue(rowIndex, "MainCategory"));
            subCategory = (ActionSubCategory)CsvHelper.ConvertToInt(data.GetValue(rowIndex, "SubCategory"));

            subCategoryName01 = data.GetValue(rowIndex, "SubCategoryName01");
            subCategoryName01 = data.GetValue(rowIndex, "SubCategoryName02");

            description = data.GetValue(rowIndex, "Description");
            costMoney = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "CostMoney"));
            costWifeHealth = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "CostWifeHealth"));

            healthChange = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "HealthChange"));
            mentalChange = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "MentalChange"));
            fatigueChange = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "FatigueChange"));
            loveChange = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "LoveChange"));

            muscleChange = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "MuscleChange"));
            techniqueChange = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "TechniqueChange"));
            concentrationChange = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "ConcentrationChange"));

            stressChange = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "StressChange"));
            skillExp = CsvHelper.ConvertToInt(data.GetValue(rowIndex, "SkillExp"));
        }
    }
}
