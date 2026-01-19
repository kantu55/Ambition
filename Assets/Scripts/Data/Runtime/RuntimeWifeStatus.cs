using Ambition.Data.Master;
using System;

namespace Ambition.Data.Runtime
{
    [Serializable]
    public class RuntimeWifeStatus
    {
        public int CookingLevel { get; private set; }
        public int CareLevel { get; private set; }

        public int PRLevel { get; private set; }

        public int CoachLevel { get; private set; }

        public RuntimeWifeStatus(WifeStatsModel master)
        {
            CookingLevel = master.InitialCooking;
            CareLevel = master.InitialCare;
            PRLevel = master.InitialPR;
            CoachLevel = master.InitialCoach;
        }

        public RuntimeWifeStatus(WifeSaveData saveData)
        {
            CookingLevel = saveData.CookingLevel;
            CareLevel = saveData.CareLevel;
            PRLevel = saveData.PRLevel;
            CoachLevel = saveData.CoachLevel;
        }

        public WifeSaveData ToSaveData()
        {
            return new WifeSaveData
            {
                CookingLevel = CookingLevel,
                CareLevel = CareLevel,
                PRLevel = PRLevel,
                CoachLevel = CoachLevel
            };
        }

        public void AddCookingExp()
        {

        }

        public void AddLooksExp()
        {

        }

        public void AddSocialExp()
        {

        }
    }
}
