using Ambition.DataStructures;
using System;
using UnityEngine;

namespace Ambition.RuntimeData
{
    [Serializable]
    public class RuntimeWifeStatus
    {
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public int CookingLevel { get; private set; }
        public int LooksLevel { get; private set; }

        public int SocialLevel { get; private set; }

        public RuntimeWifeStatus(WifeStatsModel master)
        {
            MaxHealth = master.InitialHealth;
            CurrentHealth = MaxHealth;

            CookingLevel = master.InitialCooking;
            LooksLevel = master.InitialLooks;
            SocialLevel = master.InitialSocial;
        }

        public RuntimeWifeStatus(WifeSaveData saveData)
        {
            MaxHealth = saveData.MaxHealth;
            CurrentHealth = saveData.CurrentHealth;

            CookingLevel = saveData.CookingLevel;
            LooksLevel = saveData.LooksLevel;
            SocialLevel = saveData.SocialLevel;
        }

        public WifeSaveData ToSaveData()
        {
            return new WifeSaveData
            {
                MaxHealth = this.MaxHealth,
                CurrentHealth = this.CurrentHealth,
                CookingLevel = this.CookingLevel,
                LooksLevel = this.LooksLevel,
                SocialLevel = this.SocialLevel,
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

        public void ConsumeHealth(int amount)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        }

        public void RecoverHealth(int amount)
        {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }
    }
}
