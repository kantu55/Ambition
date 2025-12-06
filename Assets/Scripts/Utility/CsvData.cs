using System.Collections.Generic;
using UnityEngine;

namespace Ambition.GameCore
{
    /// <summary>
    /// CSVのヘッダー情報と生データを保持し、列名によるアクセスを可能にするクラス
    /// </summary>
    public class CsvData
    {
        /// <summary>
        /// 列名とそのインデックス
        /// </summary>
        private Dictionary<string, int> columnNameToIndex = new Dictionary<string, int>();

        /// <summary>
        /// 生の二次元配列データを取得
        /// </summary>
        private string[,] rawData;

        /// <summary>
        /// 生の二次元配列データを取得
        /// </summary>
        public string[,] RawData => rawData;

        /// <summary>
        /// データの行数を取得
        /// </summary>
        public int RowCount => rawData.GetLength(0);

        /// <summary>
        /// データの列数を取得
        /// </summary>
        public int ColumnCount => rawData.GetLength(1);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="data">解析済みの二次元配列データ。</param>
        public CsvData(string[,] data)
        {
            this.rawData = data;

            // データが存在し、ヘッダー行がある場合のみマッピングを初期化
            if (data.GetLength(0) > 0)
            {
                // ヘッダー行を解析し、列名とそのインデックスをマッピング
                for (int i = 0; i < data.GetLength(1); i++)
                {
                    string columnName = data[0, i].Trim().ToUpper();
                    if (!columnNameToIndex.ContainsKey(columnName))
                    {
                        columnNameToIndex.Add(columnName, i);
                    }
                }
            }
        }

        /// <summary>
        /// 列名に対応するインデックスを取得
        /// </summary>
        /// <param name="columnName">CSVヘッダーに記載された列名。</param>
        /// <returns>対応する列インデックス。見つからない場合は -1。</returns>
        public int GetColumnIndex(string columnName)
        {
            string upperColumnName = columnName.ToUpper();

            if (columnNameToIndex.TryGetValue(upperColumnName, out int columnIndex))
            {
                return columnIndex;
            }

            Debug.LogError($"CSVファイルに '{columnName}' という列名が見つかりません。");
            return -1;
        }

        /// <summary>
        /// 指定した行と列名に対応する文字列データを取得
        /// </summary>
        /// <param name="row">行インデックス。</param>
        /// <param name="columnName">列名。</param>
        /// <returns>セルの値。</returns>
        public string GetValue(int row, string columnName)
        {
            int columnIndex = GetColumnIndex(columnName);

            if (columnIndex >= 0 && row < RawData.GetLength(0))
            {
                return RawData[row, columnIndex];
            }

            return string.Empty;
        }
    }
}
