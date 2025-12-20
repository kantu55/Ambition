using Ambition.DataStructures;
using System;
using UnityEngine;

namespace Ambition.GameCore
{
    /// <summary>
    /// ゲーム実行中に変動する選手のステータスを管理するクラス
    /// </summary>
    [Serializable]
    public class RuntimePlayerStatus
    {
        // --- 定数・最大値 ---
        public readonly int MAX_HEALTH = 100;
        public readonly int MAX_MENTAL = 100;
        public readonly int MAX_LOVE = 100;

        // マスタデータへの参照キー（ID）
        public int PlayerId { get; private set; }

        // 現在値
        public int CurrentHealth { get; private set; }
        public int CurrentMental { get; private set; }
        public int CurrentFatigue { get; private set; }
        public int CurrentLove { get; private set; }

        // 成長する能力値
        public int Muscle { get; private set; }
        public int Technique { get; private set; }
        public int Concentration { get; private set; }

        // 評価
        public string Evaluation { get; private set; }

        /// <summary>
        /// ニューゲーム用コンストラクタ
        /// マスタデータ（初期値）を元に生成
        /// </summary>
        public RuntimePlayerStatus(PlayerStatsModel master)
        {
            this.PlayerId = master.Id;

            this.CurrentHealth = master.Health;
            this.CurrentMental = master.Mental;
            this.CurrentFatigue = master.Fatigue;
            this.CurrentLove = master.Love;

            this.Muscle = master.Muscle;
            this.Technique = master.Technique;
            this.Concentration = master.Concentration;
            this.Evaluation = master.Evaluation;
        }

        /// <summary>
        /// セーブデータ（DTO）からの復元用コンストラクタ。
        /// </summary>
        public RuntimePlayerStatus(PlayerSaveData saveData)
        {
            this.PlayerId = saveData.Id;

            this.CurrentHealth = saveData.Health;
            this.CurrentMental = saveData.Mental;
            this.CurrentFatigue = saveData.Fatigue;
            this.CurrentLove = saveData.Love;

            this.Muscle = saveData.Muscle;
            this.Technique = saveData.Technique;
            this.Concentration = saveData.Concentration;
            this.Evaluation = saveData.Evaluation;
        }

        /// <summary>
        /// 現在の状態をセーブデータ構造体に変換
        /// </summary>
        public PlayerSaveData ToSaveData()
        {
            var master = DataManager.Instance.GetDataById<PlayerStatsModel>(this.PlayerId);
            string name = master != null ? master.Name : "Unknown";
            string pos = master != null ? master.Position.ToString() : "UNKNOWN";
            int age = master != null ? master.Age : 0;

            return new PlayerSaveData
            {
                Id = this.PlayerId,
                Name = name,
                PositionString = pos,
                Age = age,
                Health = this.CurrentHealth,
                Mental = this.CurrentMental,
                Fatigue = this.CurrentFatigue,
                Love = this.CurrentLove,
                Muscle = this.Muscle,
                Technique = this.Technique,
                Concentration = this.Concentration,
                Evaluation = this.Evaluation
            };
        }

        /// <summary>
        /// 体力を増減
        /// </summary>
        public void ChangeHealth(int amount)
        {
            this.CurrentHealth = Mathf.Clamp(this.CurrentHealth + amount, 0, MAX_HEALTH);
        }

        /// <summary>
        /// メンタルを増減
        /// </summary>
        public void ChangeMental(int amount)
        {
            this.CurrentMental = Mathf.Clamp(this.CurrentMental + amount, 0, MAX_MENTAL);
        }

        /// <summary>
        /// 疲労度を増減
        /// </summary>
        public void ChangeFatigue(int amount)
        {
            this.CurrentFatigue = Mathf.Max(0, this.CurrentFatigue + amount);
        }

        /// <summary>
        /// 夫婦仲を増減
        /// </summary>
        public void ChangeLove(int amount)
        {
            this.CurrentLove = Mathf.Clamp(this.CurrentLove + amount, 0, MAX_LOVE);
        }

        /// <summary>
        /// 能力値を成長
        /// </summary>
        public void GrowAbility(int muscleDelta, int techDelta, int concDelta)
        {
            this.Muscle += muscleDelta;
            this.Technique += techDelta;
            this.Concentration += concDelta;
        }
    }
}
