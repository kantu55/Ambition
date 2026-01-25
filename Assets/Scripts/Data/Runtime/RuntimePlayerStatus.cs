using Ambition.Core.Managers;
using Ambition.Data.Master;
using System;
using UnityEngine;

namespace Ambition.Data.Runtime
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
        public int CurrentAge { get; private set; }
        public int CurrentHealth { get; private set; }
        public int CurrentMental { get; private set; }
        public int CurrentLove { get; private set; }
        public int CurrentCondition { get; private set; }

        // 成長する能力値
        public int CurrentAbility { get; private set; }

        // 評価
        public int TeamEvaluation { get; private set; }

        // 契約金
        public int Salary { get; private set; }

        /// <summary>
        /// ニューゲーム用コンストラクタ
        /// マスタデータ（初期値）を元に生成
        /// </summary>
        public RuntimePlayerStatus(PlayerStatsModel master)
        {
            this.PlayerId = master.Id;
            this.CurrentAge = master.Age;
            this.CurrentHealth = master.Health;
            this.CurrentMental = master.Mental;
            this.CurrentLove = master.Love;
            this.CurrentCondition = master.Condition;
            this.CurrentAbility = master.Ability;
            this.TeamEvaluation = master.TeamEvaluation;
            this.Salary = master.Salary;
        }

        /// <summary>
        /// セーブデータ（DTO）からの復元用コンストラクタ。
        /// </summary>
        public RuntimePlayerStatus(PlayerSaveData saveData)
        {
            this.PlayerId = saveData.Id;
            this.CurrentAge = saveData.Age;
            this.CurrentHealth = saveData.Health;
            this.CurrentMental = saveData.Mental;
            this.CurrentLove = saveData.Love;
            this.CurrentCondition = saveData.Condition;
            this.CurrentAbility = saveData.Ability;
            this.TeamEvaluation = saveData.TeamEvaluation;
            this.Salary = saveData.Salary;
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
                Age = this.CurrentAge,
                Health = this.CurrentHealth,
                Mental = this.CurrentMental,
                Love = this.CurrentLove,
                Condition = this.CurrentCondition,
                Ability = this.CurrentAbility,
                Salary = this.Salary,
                TeamEvaluation = this.TeamEvaluation
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
        public void ChangeCondition(int amount)
        {
            this.CurrentCondition = Mathf.Max(0, this.CurrentCondition + amount);
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
        public void GrowAbility(int deltaAbility)
        {
            this.CurrentAbility += deltaAbility;
        }

        /// <summary>
        /// 加齢
        /// </summary>
        public void AddAge()
        {
            this.CurrentAge++;
        }

        /// <summary>
        /// 年棒の更新
        /// </summary>
        /// <param name="newSalary"></param>
        public void UpdateSalary(int newSalary)
        {
            this.Salary = newSalary;
        }
    }
}
