using Ambition.GameCore;
using System;

namespace Ambition.Utility
{
    /// <summary>
    /// CSVデータの解析とデータ型変換を処理するヘルパークラス。
    /// </summary>
    public static class CsvHelper
    {
        private const string NEW_LINE = "\n";
        private const char DELIMITER = ',';

        /// <summary>
        /// CSV形式の文字列データを受け取り、ヘッダーマッピング付きの CsvData オブジェクトとして解析します。
        /// </summary>
        /// <param name="csvText">CSVファイルの内容を示す文字列。</param>
        /// <returns>CsvData オブジェクト。データが空の場合は null。</returns>
        public static CsvData LoadAndParseCsvFromText(string csvText)
        {
            if (string.IsNullOrEmpty(csvText))
            {
                return null;
            }

            // 改行コードを統一して行に分割
            string[] lines = csvText.Replace("\r\n", NEW_LINE).Split(new[] { NEW_LINE }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
            {
                return null;
            }

            // ヘッダー行を解析して列数を決定
            string[] headerColumns = lines[0].Split(DELIMITER);
            int columnCount = headerColumns.Length;
            int rowCount = lines.Length;

            // CSVデータを保持する二次元配列
            string[,] parsedData = new string[rowCount, columnCount];

            for (int i = 0; i < rowCount; i++)
            {
                string[] columns = lines[i].Split(DELIMITER);

                for (int j = 0; j < columnCount; j++)
                {
                    // 列数が足りない場合は空文字列を格納（データ欠損対策）
                    string cellData = (j < columns.Length) ? columns[j].Trim() : string.Empty;
                    parsedData[i, j] = cellData;
                }
            }

            return new CsvData(parsedData);
        }

        // --- 型変換ヘルパー関数 ---

        /// <summary>
        /// 文字列を int 型に変換
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="defaultValue">変換失敗時に使用するデフォルト値。</param>
        /// <returns>変換された int 値。</returns>
        public static int ConvertToInt(string value, int defaultValue = 0)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// 文字列を float 型に変換
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="defaultValue">変換失敗時に使用するデフォルト値。</param>
        /// <returns>変換された float 値。</returns>
        public static float ConvertToFloat(string value, float defaultValue = 0.0f)
        {
            if (float.TryParse(value, out float result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// 文字列を string 型としてそのまま変換（nullチェックを含む）
        /// </summary>
        /// <param name="value">元の文字列。</param>
        /// <returns>元の文字列、または空文字。</returns>
        public static string ConvertToString(string value)
        {
            return value ?? string.Empty;
        }
    }
}