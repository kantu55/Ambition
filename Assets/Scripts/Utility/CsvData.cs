using System.Collections.Generic;

namespace Ambition.Utility
{
    /// <summary>
    /// CSVテキストを解析し、行と列ごとのデータアクセスを提供するクラス
    /// </summary>
    public class CsvData
    {
        // [行][列] の文字列データ
        private List<string[]> rows = new List<string[]>();

        // ヘッダー名から列インデックスを引く辞書
        private Dictionary<string, int> headerMap = new Dictionary<string, int>();

        public int LineCount => rows.Count;

        public CsvData(string csvText)
        {
            Parse(csvText);
        }

        /// <summary>
        /// CSVテキストをパース
        /// </summary>
        private void Parse(string text)
        {
            // 改行コード統一
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");

            string[] lines = text.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // カンマ区切り（簡易実装）
                string[] cols = line.Split(',');

                // 文字列の前後の空白除去や " の除去処理などを入れるならここ
                for (int j = 0; j < cols.Length; j++)
                {
                    cols[j] = cols[j].Trim();
                }

                rows.Add(cols);
            }

            // ヘッダー行(0行目)の解析
            if (rows.Count > 0)
            {
                string[] headers = rows[0];
                for (int i = 0; i < headers.Length; i++)
                {
                    // 重複キー対策
                    if (!headerMap.ContainsKey(headers[i]))
                    {
                        headerMap.Add(headers[i], i);
                    }
                }
            }
        }

        /// <summary>
        /// 指定した行・列名の値を取得
        /// </summary>
        public string GetValue(int rowIndex, string columnName)
        {
            if (rowIndex < 0 || rowIndex >= rows.Count)
            {
                return string.Empty;
            }

            if (headerMap.TryGetValue(columnName, out int colIndex))
            {
                string[] row = rows[rowIndex];
                if (colIndex < row.Length)
                {
                    return row[colIndex];
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 指定した行が空、またはデータ不足か判定
        /// </summary>
        public bool IsRowEmpty(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= rows.Count)
            {
                return true;
            }

            return rows[rowIndex].Length == 0;
        }
    }
}
