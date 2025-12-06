using System.Collections.Generic;
using UnityEngine;
using Ambition.DataStructures;
using Ambition.Utility;

namespace Ambition.GameCore
{
    /// <summary>
    /// ゲームで使用する全ての静的データ（CSVデータなど）を管理・保持するクラス
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        // データをキャッシュする辞書
        private readonly Dictionary<System.Type, object> dataCache = new Dictionary<System.Type, object>();

        // CSVファイルが格納されているResourcesフォルダ以下のパス
        private const string BASE_CSV_PATH = "CsvData";

        /// <summary>
        /// データモデルの型と、それに対応するCSVファイル名のマッピング。
        /// </summary>
        private readonly Dictionary<System.Type, string> csvModelMap = new Dictionary<System.Type, string>()
        {
            { typeof(PlayerStatsModel), "PlayerStats" },
        };

        /// <summary>
        /// 外部からアクセスするためのインスタンス
        /// </summary>
        public static DataManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadAllGameData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 定義された全てのマッピングに基づいてゲームデータをロード
        /// </summary>
        public void LoadAllGameData()
        {
            int totalLoadedCount = 0;

            foreach (var pair in csvModelMap)
            {
                System.Type modelType = pair.Key;
                string fileName = pair.Value;

                // ファイル名に基づきデータをロード
                totalLoadedCount += LoadCsvDataByFileName(modelType, fileName);
            }

            Debug.Log($"全てのゲームデータロードが完了しました。合計 {totalLoadedCount} 件。");
        }

        /// <summary>
        /// 指定されたファイル名からデータをロードし、キャッシュ
        /// </summary>
        /// <param name="modelType">IDataModelを実装した型。</param>
        /// <param name="fileName">ファイル名（拡張子なし）。</param>
        /// <returns>ロードした件数。</returns>
        private int LoadCsvDataByFileName(System.Type modelType, string fileName)
        {
            string fullPath = $"{BASE_CSV_PATH}/{fileName}";

            // ResourcesフォルダからTextAssetとして読み込み
            TextAsset textAsset = Resources.Load<TextAsset>(fullPath);

            if (textAsset == null)
            {
                Debug.LogError($"CSVファイルが見つかりません: {fullPath}");
                return 0;
            }

            // CSVテキストを解析
            CsvData csvData = CsvHelper.LoadAndParseCsvFromText(textAsset.text);

            if (csvData == null || csvData.RowCount <= 1)
            {
                Debug.LogError($"データロード失敗: {modelType.Name} のデータが空か解析エラーです。");
                return 0;
            }

            // 汎用リストの生成 (List<T>)
            System.Collections.IList dataList = (System.Collections.IList)System.Activator.CreateInstance(typeof(List<>).MakeGenericType(modelType));

            // データ行の処理（1行目から）
            for (int row = 1; row < csvData.RowCount; row++)
            {
                // インスタンス生成と初期化
                IDataModel model = (IDataModel)System.Activator.CreateInstance(modelType);
                model.Initialize(csvData, row);
                dataList.Add(model);
            }

            // キャッシュに保存
            dataCache[modelType] = dataList;

            Debug.Log($"{modelType.Name} のデータを {dataList.Count} 件ロードしました。");
            return dataList.Count;
        }

        /// <summary>
        /// 指定された型の全てのデータリストを取得します。
        /// </summary>
        /// <typeparam name="T">IDataModelを実装した型。</typeparam>
        /// <returns>データリスト。</returns>
        public List<T> GetDatas<T>() where T : IDataModel
        {
            if (dataCache.TryGetValue(typeof(T), out object cachedList))
            {
                return (List<T>)cachedList;
            }

            Debug.LogError($"キャッシュに {typeof(T).Name} のデータが見つかりません。");
            return new List<T>();
        }

        /// <summary>
        /// 指定されたIDを持つ単一のデータモデルを取得します。
        /// </summary>
        /// <typeparam name="T">IDataModelを実装した型。</typeparam>
        /// <param name="id">データの固有ID。</param>
        /// <returns>データモデル。見つからない場合は null。</returns>
        public T GetDataById<T>(int id) where T : IDataModel
        {
            List<T> allDatas = GetDatas<T>();
            return allDatas.Find(data => data.Id == id);
        }
    }
}