using System;
using UnityEngine;

namespace Ambition.Utility
{
    /// <summary>
    /// CSVから取得した文字列データを、安全に各種型へ変換するヘルパークラス
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>
        /// 文字列を int に変換
        /// </summary>
        public static int ConvertToInt(string value, int defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            if (int.TryParse(value, out int result))
            {
                return result;
            }

            // 変換失敗時（文字が含まれている場合など）
            Debug.LogWarning($"[CsvHelper] intへの変換に失敗しました: '{value}' -> Default: {defaultValue}");
            return defaultValue;
        }

        /// <summary>
        /// 文字列を long に変換
        /// </summary>
        public static long ConvertToLong(string value, long defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            if (long.TryParse(value, out long result))
            {
                return result;
            }

            // 変換失敗時（文字が含まれている場合など）
            Debug.LogWarning($"[CsvHelper] longへの変換に失敗しました: '{value}' -> Default: {defaultValue}");
            return defaultValue;
        }

        /// <summary>
        /// 文字列を float に変換
        /// </summary>
        public static float ConvertToFloat(string value, float defaultValue = 0.0f)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            if (float.TryParse(value, out float result))
            {
                return result;
            }

            Debug.LogWarning($"[CsvHelper] floatへの変換に失敗しました: '{value}' -> Default: {defaultValue}");
            return defaultValue;
        }

        /// <summary>
        /// 文字列を bool に変換
        /// /// "true", "TRUE", "1" などを true と判定
        /// </summary>
        public static bool ConvertToBool(string value, bool defaultValue = false)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            if (value == "1")
            {
                // "1" の場合は true として扱う
                return true;
            }
            else if (value == "0")
            {
                // "0" の場合は false として扱う
                return false;
            }

            if (bool.TryParse(value, out bool result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// 文字列を Enum に変換
        /// 数値文字列("1")と名前文字列("Support")の両方に対応します。
        /// </summary>
        /// <typeparam name="T">対象のEnum型</typeparam>
        public static T ConvertToEnum<T>(string value, T defaultValue = default) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            // 1. 数値としてパースできるか試す ("1" -> Enum.Value1)
            if (int.TryParse(value, out int intValue))
            {
                if (Enum.IsDefined(typeof(T), intValue))
                {
                    return (T)Enum.ToObject(typeof(T), intValue);
                }
            }

            // 2. 名前としてパースできるか試す ("Support" -> Enum.Support)
            if (Enum.TryParse(value, true, out T result))
            {
                return result;
            }

            Debug.LogWarning($"[CsvHelper] Enum({typeof(T).Name})への変換に失敗しました: '{value}'");
            return defaultValue;
        }

        /// <summary>
        /// 文字列の前後の空白を除去
        /// nullの場合は空文字を返す
        /// </summary>
        public static string SanitizeString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Trim();
        }
    }
}