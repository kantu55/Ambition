using System.Collections.Generic;
using System.Linq;
using Ambition.Data.Master;
using Ambition.Data.Master.Event;
using UnityEngine;

namespace Ambition.Data.Runtime
{
    /// <summary>
    /// 1年間（12ターン）のEventMasterイベントスケジュールを管理するクラス。
    /// ゲーム開始時または年切り替え時に生成し、どのターン（月）にどのイベントが発生するかを保持する。
    /// </summary>
    [Serializable]
    public class RuntimeEventSchedule
    {
        private const int TURNS_PER_YEAR = 12;

        /// <summary>
        /// 月（インデックス0=1月）ごとのEventId。-1はイベントなし。
        /// </summary>
        private int[] eventIdByTurn;

        /// <summary>
        /// EventBlockとEventMasterデータを使い、指定年のスケジュールをランダム生成する。
        /// </summary>
        public RuntimeEventSchedule(int year, List<EventBlock> eventBlocks, List<EventMaster> eventMasters)
        {
            eventIdByTurn = new int[TURNS_PER_YEAR];
            for (int i = 0; i < TURNS_PER_YEAR; i++)
            {
                eventIdByTurn[i] = -1;
            }

            var block = eventBlocks.FirstOrDefault(b => b.Year == year);
            if (block == null)
            {
                Debug.LogWarning($"[RuntimeEventSchedule] Year {year} のEventBlockが見つかりません。");
                return;
            }

            var positiveEvents = eventMasters.Where(e => e.EventType == 1).ToList();
            var negativeEvents = eventMasters.Where(e => e.EventType == 2).ToList();

            List<int> selectedIds = new List<int>();

            for (int i = 0; i < block.PositiveCount && positiveEvents.Count > 0; i++)
            {
                int idx = Random.Range(0, positiveEvents.Count);
                selectedIds.Add(positiveEvents[idx].EventId);
                positiveEvents.RemoveAt(idx);
            }

            for (int i = 0; i < block.NegativeCount && negativeEvents.Count > 0; i++)
            {
                int idx = Random.Range(0, negativeEvents.Count);
                selectedIds.Add(negativeEvents[idx].EventId);
                negativeEvents.RemoveAt(idx);
            }

            List<int> availableSlots = Enumerable.Range(0, TURNS_PER_YEAR).ToList();
            foreach (int eventId in selectedIds)
            {
                int slotIdx = Random.Range(0, availableSlots.Count);
                eventIdByTurn[availableSlots[slotIdx]] = eventId;
                availableSlots.RemoveAt(slotIdx);
            }

            Debug.Log($"[RuntimeEventSchedule] Year {year} のスケジュールを生成しました。" +
                      $"（ポジティブ:{block.PositiveCount}件、ネガティブ:{block.NegativeCount}件）");
        }

        /// <summary>
        /// セーブデータからスケジュールを復元する。
        /// </summary>
        public RuntimeEventSchedule(EventScheduleSaveData saveData)
        {
            if (saveData.EventIdByTurn != null && saveData.EventIdByTurn.Length == TURNS_PER_YEAR)
            {
                eventIdByTurn = (int[])saveData.EventIdByTurn.Clone();
            }
            else
            {
                eventIdByTurn = new int[TURNS_PER_YEAR];
                for (int i = 0; i < TURNS_PER_YEAR; i++)
                {
                    eventIdByTurn[i] = -1;
                }
            }
        }

        /// <summary>
        /// 指定した月（1〜12）に対応するEventIdを返す。イベントがない月は-1を返す。
        /// </summary>
        public int GetEventIdForMonth(int month)
        {
            if (month < 1 || month > TURNS_PER_YEAR)
            {
                return -1;
            }

            return eventIdByTurn[month - 1];
        }

        /// <summary>
        /// セーブデータ用の構造体に変換する。
        /// </summary>
        public EventScheduleSaveData ToSaveData()
        {
            return new EventScheduleSaveData
            {
                EventIdByTurn = (int[])eventIdByTurn.Clone()
            };
        }
    }
}
