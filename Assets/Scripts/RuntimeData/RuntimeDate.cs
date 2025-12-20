using UnityEngine.Rendering;
using Ambition.DataStructures;

namespace Ambition.RuntimeData
{
    [System.Serializable]
    public class RuntimeDate
    {
        private readonly int MONTHS_IN_YEAR = 12;

        public int Year { get; private set; }
        public int Month { get; private set; }

        public RuntimeDate(int year, int month)
        {
            Year = year;
            Month = month;
        }

        public RuntimeDate(DateSaveData saveData)
        {
            Year = saveData.Year;
            Month = saveData.Month;
        }

        public DateSaveData ToSaveData()
        {
            return new DateSaveData
            {
                Year = Year,
                Month = Month,
            };
        }

        /// <summary>
        /// 月を進める
        /// </summary>
        public void AdvanceMonth()
        {
            Month++;
            if (Month > MONTHS_IN_YEAR)
            {
                Month = 1;
                Year++;
            }
        }

        /// <summary>
        /// 特定の期間を進める場合（デバッグやスキップ機能用）
        /// </summary>
        public void AddMonths(int months)
        {
            int totalMonths = (Year * MONTHS_IN_YEAR + (Month - 1)) + months;
            Year = totalMonths / MONTHS_IN_YEAR;
            Month = (totalMonths % MONTHS_IN_YEAR) + 1;
        }
    }
}