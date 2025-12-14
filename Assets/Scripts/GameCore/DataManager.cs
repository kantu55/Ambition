using Ambition.DataStructures;
using Ambition.Utility;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<Type, IList> masterDataCache = new Dictionary<Type, IList>();

        /// <summary>
        /// データモデルの型と、それに対応するAddressablesのアドレス（キー）のマッピング
        /// </summary>
        private readonly Dictionary<Type, string> csvModelMap = new Dictionary<Type, string>()
        {
            { typeof(PlayerStatsModel), "PlayerStats" },
            { typeof(WifeStatsModel), "WifeStats" },
            { typeof(HousingModel), "Housing" },
            { typeof(FixedCostModel), "FixedCost" },
            { typeof(BudgetModel), "Budget" },
            { typeof(GameSettingModel), "GameSettings" },
            { typeof(WifeActionModel), "WifeActions" },
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
        /// 登録されている全てのCSVデータを非同期で読み込み
        /// </summary>
        public async UniTask LoadAllGameDataAsync()
        {
            masterDataCache.Clear();
            var tasks = new List<UniTask>();

            foreach (var pair in csvModelMap)
            {
                tasks.Add(LoadDataByTypeAsync(pair.Key, pair.Value));
            }

            await UniTask.WhenAll(tasks);

            Debug.Log("[DataManager] 全マスタデータの読み込みが完了しました。");
        }

        /// <summary>
        /// 型情報をもとに適切なロードメソッドを呼び出すヘルパー。
        /// </summary>
        private async UniTask LoadDataByTypeAsync(Type type, string address)
        {
            if (type == typeof(PlayerStatsModel))
            {
                await LoadCsvDataByAddressAsync<PlayerStatsModel>(address);
            }
            else if (type == typeof(WifeActionModel))
            {
                await LoadCsvDataByAddressAsync<WifeActionModel>(address);
            }
            else if (type == typeof(GameSettingModel))
            {
                await LoadCsvDataByAddressAsync<GameSettingModel>(address);
            }
            else if (type == typeof(HousingModel))
            {
                await LoadCsvDataByAddressAsync<HousingModel>(address);
            }
            else if (type == typeof(WifeStatsModel))
            {
                await LoadCsvDataByAddressAsync<WifeStatsModel>(address);
            }
        }

        /// <summary>
        /// AddressablesからCSVを読み込み、指定されたモデルのリストに変換
        /// </summary>
        private async UniTask LoadCsvDataByAddressAsync<T>(string address) where T : IDataModel, new()
        {
            var handle = Addressables.LoadAssetAsync<TextAsset>(address);
            TextAsset csvAsset = await handle.ToUniTask();
            if (csvAsset == null)
            {
                Debug.LogError($"Addressables ロード失敗: アドレス '{address}' が見つかりません。");
                return;
            }

            CsvData csvData = new CsvData(csvAsset.text);
            List<T> list = new List<T>();

            // データ変換ループ
            // 1行目はヘッダーなので index 1 から開始
            for (int i = 1; i < csvData.LineCount; i++)
            {
                if (csvData.IsRowEmpty(i))
                {
                    continue;
                }

                T model = new T();

                model.Initialize(csvData, i);
                list.Add(model);
            }

            if (masterDataCache.ContainsKey(typeof(T)))
            {
                masterDataCache[typeof(T)] = list;
            }
            else
            {
                masterDataCache.Add(typeof(T), list);
            }

            // データを取り出した後のTextAssetは不要なのでハンドルをリリース
            Addressables.Release(handle);

            Debug.Log($"[DataManager] ロード完了: {typeof(T).Name} ({list.Count}件)");
        }

        /// <summary>
        /// 指定された型の全てのデータリストを取得
        /// </summary>
        /// <typeparam name="T">IDataModelを実装した型。</typeparam>
        /// <returns>データリスト。</returns>
        public List<T> GetDatas<T>() where T : class, IDataModel
        {
            if (masterDataCache.TryGetValue(typeof(T), out IList list))
            {
                return list as List<T>;
            }

            Debug.LogWarning($"[DataManager] データがロードされていません: {typeof(T).Name}");
            return new List<T>();
        }

        /// <summary>
        /// 指定されたIDを持つ単一のデータモデルを取得
        /// </summary>
        /// <typeparam name="T">IDataModelを実装した型。</typeparam>
        /// <param name="id">データの固有ID。</param>
        /// <returns>データモデル。見つからない場合は null。</returns>
        public T GetDataById<T>(int id) where T : class, IDataModel
        {
            List<T> allDatas = GetDatas<T>();
            return allDatas.Find(data => data.Id == id);
        }

        /// <summary>
        /// 指定されたメインカテゴリに属する行動リストを取得
        /// </summary>
        public List<WifeActionModel> GetActionsByMainCategory(WifeActionModel.ActionMainCategory category)
        {
            var allActions = GetDatas<WifeActionModel>();
            return allActions.Where(a => a.MainCategory == category).ToList();
        }

        /// <summary>
        /// 指定されたサブカテゴリに属する行動リストを取得
        /// </summary>
        public List<WifeActionModel> GetActionsBySubCategory(WifeActionModel.ActionSubCategory category)
        {
            var allActions = GetDatas<WifeActionModel>();
            return allActions.Where(a => a.SubCategory == category).ToList();
        }
    }
}