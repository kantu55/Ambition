using System;

namespace Ambition.DataStructures
{
    /// <summary>
    /// セーブファイルに記録されるデータ構造
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        // JsonUtilityでのシリアライズ用のため、フィールドはpublic変数

        public int CurrentTurn;

        public PlayerSaveData PlayerData;
        public WifeSaveData WifeData;
        public EnvironmentSaveData EnvironmentData;
        public BudgetSaveData BudgetData;
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

    /// <summary>
    /// 妻のステータス保存用構造体
    /// </summary>
    [Serializable]
    public struct WifeSaveData
    {
        public int CurrentHealth;
        public int MaxHealth;
        public int CookingLevel;
        public int LooksLevel;
        public int SocialLevel;
    }

    /// <summary>
    /// 家の環境のステータス保存構造体
    /// </summary>
    [Serializable]
    public struct EnvironmentSaveData
    {
        public int CurrentHouseId;
        public int BedLevel;
        public int GymLevel;
        public int MealRank;
    }

    /// <summary>
    /// 家賃のステータス保存構造体
    /// </summary>
    [Serializable]
    public struct BudgetSaveData
    {
        public long CurrentSavings;
        public FixedCostSaveData FixedCostSaveData;
    }

    /// <summary>
    /// 固定費のステータス保存構造体
    /// </summary>
    [Serializable]
    public struct FixedCostSaveData
    {
        public int Rent;
        public int FoodCost;
        public int Tax;
        public int Insurance;
        public int Maintenance;
    }
}
