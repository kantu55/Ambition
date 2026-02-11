using Ambition.Data.Master;
using UnityEngine;

namespace Ambition.Data.Runtime
{
    [System.Serializable]
    public class RuntimeReputation
    {
        // --- 定数・最大値 ---
        public readonly int MAX_LOVE = 100;
        public readonly int MAX_TEAM_EVALUATION = 100;
        public readonly int MAX_PUBLIC_EYE = 100;
        public readonly int MAX_CP = 1700;

        public int CurrentLove { get; private set; }
        public int CurrentTeamEvaluation { get; private set; }
        public int CurrentPublicEye { get; private set; }
        public int CurrentCP { get; private set; } // 契約用の累積評価ポイント

        public RuntimeReputation()
        {
            this.CurrentLove = 60;
            this.CurrentTeamEvaluation = 50;
            this.CurrentPublicEye = 0;
            this.CurrentCP = 0;
        }

        public RuntimeReputation(ReputationSaveData saveData)
        {
            this.CurrentLove = saveData.Love;
            this.CurrentTeamEvaluation = saveData.TeamEvaluation;
            this.CurrentPublicEye = saveData.PublicEye;
            this.CurrentCP = saveData.CP;
        }

        public ReputationSaveData ToSaveData()
        {
            return new ReputationSaveData
            {
                Love = CurrentLove,
                TeamEvaluation = CurrentTeamEvaluation,
                PublicEye = CurrentPublicEye,
                CP = CurrentCP
            };
        }

        /// <summary>
        /// 夫婦仲を増減
        /// </summary>
        public void ChangeLove(int amount)
        {
            this.CurrentLove = Mathf.Clamp(this.CurrentLove + amount, 0, MAX_LOVE);
        }

        /// <summary>
        /// チーム評価を増減
        /// </summary>
        public void ChangeTeamEvaluation(int amount)
        {
            this.CurrentTeamEvaluation = Mathf.Clamp(this.CurrentTeamEvaluation + amount, 0, MAX_TEAM_EVALUATION);
        }

        /// <summary>
        /// 世間の目を増減
        /// </summary>
        public void ChangePublicEye(int amount)
        {
            this.CurrentPublicEye = Mathf.Clamp(this.CurrentPublicEye + amount, 0, MAX_PUBLIC_EYE);
        }

        public void ChangeCP(int amount)
        {
            this.CurrentCP = Mathf.Clamp(this.CurrentCP + amount, 0, MAX_CP);
        }
    }
}
