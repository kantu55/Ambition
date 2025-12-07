using Ambition.DataStructures;
using Ambition.Utility;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ambition.GameCore
{
    /// <summary>
    /// ゲームで使用する全ての静的データ（CSVデータなど）を管理・保持するクラス
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        // データをキャッシュする辞書
        private readonly Dictionary<System.Type, object> dataCache = new Dictionary<System.Type, object>();

        /// <summary>
        /// データモデルの型と、それに対応するAddressablesのアドレス（キー）のマッピング
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
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 定義された全てのマッピングに基づいてゲームデータをロード
        /// </summary>
        public async UniTask LoadAllGameDataAsync()
        {
            int totalLoadedCount = 0;

            foreach (var pair in csvModelMap)
            {
                System.Type modelType = pair.Key;
                string address = pair.Value;
                totalLoadedCount += await LoadCsvDataByAddressAsync(modelType, address);
            }

            Debug.Log($"全てのゲームデータロードが完了しました。合計 {totalLoadedCount} 件。");
        }

        /// <summary>
        /// 指定されたアドレスからデータをAddressablesでロードし、キャッシュ
        /// </summary>
        /// <param name="modelType">IDataModelを実装した型</param>
        /// <param name="address">Addressables名</param>
        /// <returns>ロードした件数。</returns>
        private async UniTask<int> LoadCsvDataByAddressAsync(System.Type modelType, string address)
        {
            var handle = Addressables.LoadAssetAsync<TextAsset>(address);
            TextAsset textAsset = await handle.ToUniTask();
            if (textAsset == null)
            {
                Debug.LogError($"Addressables ロード失敗: アドレス '{address}' が見つかりません。");
                return 0;
            }

            CsvData csvData = CsvHelper.LoadAndParseCsvFromText(textAsset.text);
            if (csvData == null || csvData.RowCount <= 1)
            {
                Debug.LogError($"データロード失敗: {modelType.Name}");
                return 0;
            }

            System.Collections.IList dataList = (System.Collections.IList)System.Activator.CreateInstance(typeof(List<>).MakeGenericType(modelType));

            for (int row = 1; row < csvData.RowCount; row++)
            {
                IDataModel model = (IDataModel)System.Activator.CreateInstance(modelType);
                model.Initialize(csvData, row);
                dataList.Add(model);
            }

            dataCache[modelType] = dataList;
            return dataList.Count;
        }

        /// <summary>
        /// 指定された型の全てのデータリストを取得
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
        /// 指定されたIDを持つ単一のデータモデルを取得
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