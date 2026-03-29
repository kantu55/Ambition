using Ambition.Data.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ambition.Data.Runtime
{
    [Serializable]
    public class RuntimeEnvironmentStatus
    {
        public int CurrentHouseId { get; private set; }
        public int BedLevel { get; private set; }
        public int GymLevel { get; private set; }
        public int MealRank { get; private set; }
        public List<RuntimeOwnedEquipment> OwnedEquipments { get; private set; }

        public RuntimeEnvironmentStatus(int initialHouseId)
        {
            CurrentHouseId = initialHouseId;
            BedLevel = 1;
            GymLevel = 0; // 初期は購入していない想定なのでレベル0
            MealRank = 0;
            OwnedEquipments = new List<RuntimeOwnedEquipment>();
        }

        public RuntimeEnvironmentStatus(EnvironmentSaveData saveData)
        {
            CurrentHouseId = saveData.CurrentHouseId;
            BedLevel = saveData.BedLevel;
            GymLevel = saveData.GymLevel;
            MealRank = saveData.MealRank;
            OwnedEquipments = saveData.OwnedEquipments != null
                ? saveData.OwnedEquipments.Select(e => new RuntimeOwnedEquipment(e.EquipmentLevel, e.DurabilityMonths)).ToList()
                : new List<RuntimeOwnedEquipment>();
        }

        public EnvironmentSaveData ToSaveData()
        {
            var ownedEquipmentsSaveData = OwnedEquipments != null && OwnedEquipments.Count > 0
                ? OwnedEquipments.Select(e => new OwnedEquipmentSaveData { EquipmentLevel = e.EquipId, DurabilityMonths = e.RemainingDurabilityMonths }).ToArray()
                : Array.Empty<OwnedEquipmentSaveData>();
            return new EnvironmentSaveData
            {
                CurrentHouseId = CurrentHouseId,
                BedLevel = BedLevel,
                GymLevel = GymLevel,
                MealRank = MealRank,
                OwnedEquipments = ownedEquipmentsSaveData
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

        /// <summary>
        /// 設備の追加
        /// </summary>
        public void AddEquipment(EquipmentModel equipment)
        {
            if (OwnedEquipments == null)
            {
                OwnedEquipments = new List<RuntimeOwnedEquipment>();
            }

            OwnedEquipments.Add(new RuntimeOwnedEquipment(equipment.Id, equipment.DurabilityMonths));
        }
    }
}