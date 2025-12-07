using System;
using System.Collections.Generic;

namespace Ambition.DataStructures
{
    /// <summary>
    /// セーブファイルに記録されるデータ構造
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        // JsonUtilityでのシリアライズ用のため、フィールドはpublic変数

        public PlayerSaveData PlayerData;
    }

    /// <summary>
    /// 選手のステータス保存用構造体。
    /// </summary>
    [Serializable]
    public struct PlayerSaveData
    {
        public int Id;
        public string Name;
        public string PositionString;
        public int Age;
        public int Health;
        public int Mental;
        public int Fatigue;
        public int Muscle;
        public int Technique;
        public int Concentration;
        public string Evaluation;
        public int Salary;
        public int Love;
    }
}
