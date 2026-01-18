using Ambition.Data.Master;
using Ambition.Utils;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ambition.Core.Managers
{
    /// <summary>
    /// ゲームで使用する全ての静的データ（CSVデータなど）を管理・保持するクラス
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        /// <summary>
        /// Addressablesのラベル名
        /// </summary>
        private const string LABEL_MASTER_DATA = "MasterData";

        /// <summary>
        /// データをキャッシュする辞書
        /// </summary>
        private Dictionary<Type, IList> masterDataCache = new Dictionary<Type, IList>();

        /// <summary>
        /// データモデルの型と、それに対応するAddressablesのアドレス（キー）のマッピング
        /// </summary>
        private readonly Dictionary<Type, string> csvModelMap = new Dictionary<Type, string>()
        {
            { typeof(PlayerStatsModel), "PlayerStats" },
            { typeof(WifeStatsModel), "WifeStats" },
            { typeof(HousingModel), "Housing" },
            { typeof(GameSettingModel), "GameSettings" },
            { typeof(WifeActionModel), "WifeActions" },
            { typeof(FoodMitModel), "FoodMit" },
            { typeof(FoodModel), "MasterFood" },
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

            Debug.Log("[DataManager] 全マスタデータの読み込みを開始します...");

            Dictionary<string, string> loadedTexts = await AssetLoader.LoadAllTextDataByLabelAndReleaseAsync(LABEL_MASTER_DATA);
            if (loadedTexts == null || loadedTexts.Count == 0)
            {
                Debug.LogWarning($"[DataManager] ラベル '{LABEL_MASTER_DATA}' のデータが見つかりませんでした。");
                return;
            }

            foreach (var pair in csvModelMap)
            {
                Type type = pair.Key;
                string fileName = pair.Value;
                if (loadedTexts.TryGetValue(fileName, out string csvContent))
                {
                    ParseAndCache(type, csvContent);
                }
                else
                {
                    Debug.LogError($"[DataManager] 必須データが見つかりません: {fileName} (Type: {type.Name})");
                }
            }

            Debug.Log("[DataManager] 全マスタデータの読み込みが完了しました。");
        }

        /// <summary>
        /// 型情報をもとに適切なロードメソッドを呼び出すヘルパー。
        /// </summary>
        private void ParseAndCache(Type type, string address)
        {
            if (type == typeof(PlayerStatsModel))
            {
                ParseCsvData<PlayerStatsModel>(address);
            }
            else if (type == typeof(WifeActionModel))
            {
                ParseCsvData<WifeActionModel>(address);
            }
            else if (type == typeof(GameSettingModel))
            {
                ParseCsvData<GameSettingModel>(address);
            }
            else if (type == typeof(HousingModel))
            {
                ParseCsvData<HousingModel>(address);
            }
            else if (type == typeof(WifeStatsModel))
            {
                ParseCsvData<WifeStatsModel>(address);
            }
            else if (type == typeof(FoodModel))
            {
                 ParseCsvData<FoodModel>(address);
            }
            else if (type == typeof(FoodMitModel))
            {
                ParseCsvData<FoodMitModel>(address);
            }
            else
            {
                Debug.LogError($"[DataManager] 未対応のデータモデル型です: {type.Name}");
            }
        }

        /// <summary>
        /// AddressablesからCSVを読み込み、指定されたモデルのリストに変換
        /// </summary>
        private void ParseCsvData<T>(string csvContent) where T : IDataModel, new()
        {
            CsvData csvData = new CsvData(csvContent);
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

            Debug.Log($"[DataManager] データ構築完了: {typeof(T).Name} ({list.Count}件)");
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
            return allActions.Where(a => a.GetMainCategory() == category).ToList();
        }
    }
}