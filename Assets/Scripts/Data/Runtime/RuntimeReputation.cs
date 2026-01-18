using Ambition.Data.Master;
using UnityEngine;

namespace Ambition.Data.Runtime
{
    [System.Serializable]
    public class RuntimeReputation
    {
        // --- 定数・最大値 ---
        public readonly int MAX_LOVE = 100;

        public int CurrentLove { get; private set; }
        public int CurrentTeamEvaluation { get; private set; }
        public int CurrentPublicEye { get; private set; }

        public RuntimeReputation()
        {
            this.CurrentLove = 60;
            this.CurrentTeamEvaluation = 50;
            this.CurrentPublicEye = 0;
        }

        public RuntimeReputation(ReputationSaveData saveData)
        {
            this.CurrentLove = saveData.Love;
            this.CurrentTeamEvaluation = saveData.TeamEvaluation;
            this.CurrentPublicEye = saveData.PublicEye;
        }

        public ReputationSaveData ToSaveData()
        {
            return new ReputationSaveData
            {
                Love = CurrentLove,
                TeamEvaluation = CurrentTeamEvaluation,
                PublicEye = CurrentPublicEye
            };
        }

        /// <summary>
        /// 夫婦仲を増減
        /// </summary>
        public void ChangeLove(int amount)
        {
            this.CurrentLove = Mathf.Clamp(this.CurrentLove + amount, 0, MAX_LOVE);
        }
    }
}
