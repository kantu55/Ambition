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
            CARE,
            SUPPORT,
            REST,
            DISCIPLINE,
            SNS,
            TALK,
        }

        // --- メンバ変数 ---
        private int id;
        private string actionId;
        private string name;
        private string tag;
        private string req;

        // コスト
        private int cashCost;

        // 夫への効果
        private int deltaHP;
        private int deltaMP;
        private int deltaCOND;
        private int deltaLove;
        private int deltaPublicEye;
        private int deltaTeamEvaluation;

        // 成長補正
        private float growthAdd;
        private float growthMul;

        // その他
        private string successModel;
        private string note;

        // --- プロパティ ---
        public int Id => id;
        public string ActionId => actionId;
        public string Name => name;
        public string Tag => tag;
        public string Req => req;
        public int CashCost => cashCost;
        public int DeltaHP => deltaHP;
        public int DeltaMP => deltaMP;
        public int DeltaCOND => deltaCOND;
        public int DeltaLove => deltaLove;
        public int DeltaPublicEye => deltaPublicEye;
        public int DeltaTeamEvaluation => deltaTeamEvaluation;
        public float GrowthAdd => growthAdd;
        public float GrowthMul => growthMul;



        public void Initialize(CsvData data, int rowIndex)
        {
            actionId = data.GetValue(rowIndex, "action_id");
            name = data.GetValue(rowIndex, "name");
            tag = data.GetValue(rowIndex, "tag");
            req = data.GetValue(rowIndex, "req");

            cashCost = data.GetValueToInt(rowIndex, "cash_cost");
            deltaHP = data.GetValueToInt(rowIndex, "delta_HP");
            deltaMP = data.GetValueToInt(rowIndex, "delta_MP");
            deltaCOND = data.GetValueToInt(rowIndex, "delta_COND");
            deltaLove = data.GetValueToInt(rowIndex, "delta_RL");
            deltaPublicEye = data.GetValueToInt(rowIndex, "delta_RP");
            deltaTeamEvaluation = data.GetValueToInt(rowIndex, "delta_T");
            growthAdd = data.GetValueToFloat(rowIndex, "growth_add");
            growthMul = data.GetValueToFloat(rowIndex, "growth_mul");
        }

        public ActionMainCategory GetMainCategory()
        {
            return tag switch
            {
                "CARE" => ActionMainCategory.CARE,
                "SUPPORT" => ActionMainCategory.SUPPORT,
                "REST" => ActionMainCategory.REST,
                "DISCIPLINE" => ActionMainCategory.DISCIPLINE,
                "SNS" => ActionMainCategory.SNS,
                "TALK" => ActionMainCategory.TALK,
                _ => ActionMainCategory.NONE,
            };
        }
    }
}
