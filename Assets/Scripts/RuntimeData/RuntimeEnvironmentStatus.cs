using UnityEngine;
using UnityEditor;
using System;
using Ambition.DataStructures;

namespace Ambition.RuntimeData
{
    [Serializable]
    public class RuntimeEnvironmentStatus
    {
        public int CurrentHouseId { get; private set; }
        public int BedLevel { get; private set; }
        public int GymLevel { get; private set; }
        public int MealRank { get; private set; }

        public RuntimeEnvironmentStatus(int initialHouseId)
        {
            CurrentHouseId = initialHouseId;
            BedLevel = 1;
            GymLevel = 0; // 初期は購入していない想定なのでレベル0
            MealRank = 0;
        }

        public RuntimeEnvironmentStatus(EnvironmentSaveData saveData)
        {
            CurrentHouseId = saveData.CurrentHouseId;
            BedLevel = saveData.BedLevel;
            GymLevel = saveData.GymLevel;
            MealRank = saveData.MealRank;
        }

        public EnvironmentSaveData ToSaveData()
        {
            return new EnvironmentSaveData
            {
                CurrentHouseId = CurrentHouseId,
                BedLevel = BedLevel,
                GymLevel = GymLevel,
                MealRank = MealRank,
            };
        }

        /// <summary>
        /// 引っ越し
        /// </summary>
        /// <param name="newHouseId"></param>
        public void MoveHouse(int newHouseId)
        {
            CurrentHouseId = newHouseId;
        }

        /// <summary>
        /// 食事ランクを変更
        /// </summary>
        public void ChangeMealRank(int newRank)
        {
            MealRank = Mathf.Clamp(newRank, 0, 3);
        }
    }
}