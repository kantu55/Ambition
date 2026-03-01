using System;
using System.Collections.Generic;
using System.Linq;
using Ambition.Data.Master;
using Ambition.Data.Master.Event;
using UnityEngine;

namespace Ambition.Data.Runtime
{
    /// <summary>
    /// 1年間（12ターン）のEventMasterイベントスケジュールを管理するクラス
    /// ゲーム開始時または年切り替え時に生成し、どのターン（月）にどのイベントが発生するかを保持
    /// </summary>
    [Serializable]
    public class RuntimeEventSchedule
    {
        private const int TURNS_PER_YEAR = 12;

        /// <summary>
        /// 月ごとのEventId
        /// -1はイベントなし
        /// </summary>
        private int[] eventIdByTurn;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RuntimeEventSchedule(int year, List<EventMaster> eventMasters, List<EventBlock> eventBlocks)
        {
            eventIdByTurn = new int[TURNS_PER_YEAR];
            for (int i = 0; i < TURNS_PER_YEAR; i++)
            {
                eventIdByTurn[i] = -1;
            }

            var block = eventBlocks.FirstOrDefault(b => b.Year == year);
            if (block == null)
            {
                Debug.LogWarning($"[RuntimeEventSchedule] 年 {year} のイベントブロックが見つかりませんでした。");
                return;
            }

            var positiveEvents = eventMasters.Where(e => e.EventType == 1).ToList();
            var negativeEvents = eventMasters.Where(e => e.EventType == 2).ToList();

            List<int> selectedIds = new List<int>();

            // ポジティブイベントをランダムに選択
            for (int i = 0; i < block.PositiveCount; i++)
            {
                if (positiveEvents.Count == 0)
                {
                    break;
                }

                var selected = positiveEvents[UnityEngine.Random.Range(0, positiveEvents.Count)];
                selectedIds.Add(selected.EventId);
                positiveEvents.Remove(selected);
            }

            // ネガティブイベントをランダムに選択
            for (int i = 0; i < block.NegativeCount; i++)
            {
                if (negativeEvents.Count == 0)
                {
                    break;
                }

                var selected = negativeEvents[UnityEngine.Random.Range(0, negativeEvents.Count)];
                selectedIds.Add(selected.EventId);
                negativeEvents.Remove(selected);
            }

            // 選択されたイベントを月にランダムに割り当て
            List<int> availableSlots = Enumerable.Range(0, TURNS_PER_YEAR).ToList();
            foreach (int eventId in selectedIds)
            {
                if (availableSlots.Count == 0)
                {
                    break;
                }

                int monthIndex = UnityEngine.Random.Range(0, availableSlots.Count);
                int month = availableSlots[monthIndex];
                eventIdByTurn[month] = eventId;
                availableSlots.RemoveAt(monthIndex);
            }

            Debug.Log($"[RuntimeEventSchedule] 年 {year} のイベントスケジュールが生成されました。" +
                $"（ポジティブ:{block.PositiveCount}件、ネガティブ:{block.NegativeCount}件）");
        }

        /// <summary>
        /// セーブデータからスケジュールを復元するコンストラクタ
        /// </summary>
        public RuntimeEventSchedule(EventScheduleSaveData saveData)
        {
            if (saveData.EventIdByTurn != null && saveData.EventIdByTurn.Length == TURNS_PER_YEAR)
            {
                eventIdByTurn = saveData.EventIdByTurn;

            }
            else
            {
                Debug.LogWarning("[RuntimeEventSchedule] セーブデータのイベントスケジュールが不正です。新規スケジュールを生成します。");
                eventIdByTurn = new int[TURNS_PER_YEAR];
                for (int i = 0; i < TURNS_PER_YEAR; i++)
                {
                    eventIdByTurn[i] = -1;
                }
            }
        }

        /// <summary>
        /// 指定した月に対応するEventIdを返す
        /// イベントがない月は-1を返す
        /// </summary>
        public int GetEventIdForMonth(int month)
        {
            if (month < 1 || month > TURNS_PER_YEAR)
            {
                Debug.LogError($"[RuntimeEventSchedule] 月の指定が不正です: {month}");
                return -1;
            }

            return eventIdByTurn[month - 1];
        }

        /// <summary>
        /// セーブデータ用の構造体に変換する
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
