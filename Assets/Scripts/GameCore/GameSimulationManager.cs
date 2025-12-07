using UnityEngine;
using Ambition.DataStructures;
using System.IO;
using Cysharp.Threading.Tasks;
using Assets.Scripts.DataStructures;

namespace Ambition.GameCore
{
    /// <summary>
    /// ゲームの動的な状態（夫、妻、環境、家計、ターン）を一元管理し、
    /// 行動の実行やセーブ・ロードを行う中核クラス
    /// </summary>
    public class GameSimulationManager : MonoBehaviour
    {
        /// <summary>
        /// インスタンス
        /// </summary>
        public static GameSimulationManager Instance { get; private set; }

        // --- 定数 ---

        /// <summary>
        /// セーブファイル名
        /// </summary>
        private const string SAVE_FILE_NAME = "save_data.json";

        // --- メンバ変数 ---

        /// <summary>
        /// 現在育成中の選手（夫）
        /// </summary>
        private RuntimePlayerStatus husband;

        // --- プロパティ ---

        public RuntimePlayerStatus Husband => husband;

        /// <summary>
        /// 夫の名前を取得するヘルパープロパティ
        /// </summary>
        public string HusbandName
        {
            get
            {
                if (husband == null)
                {
                    return "";
                }

                var master = DataManager.Instance.GetDataById<PlayerStatsModel>(husband.PlayerId);
                return master != null ? master.Name : "Unknown";
            }
        }

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

        // --- ゲーム開始 / 初期化 ---

        /// <summary>
        /// ゲーム開始時の初期化処理。
        /// </summary>
        /// <param name="playerId">育成対象の選手ID</param>
        public void StartNewGame(int playerId)
        {
            PlayerStatsModel master = DataManager.Instance.GetDataById<PlayerStatsModel>(playerId);
            husband = new RuntimePlayerStatus(master);

            Debug.Log($"新規ゲーム開始: 選手 {master.Name}");
        }

        // --- セーブ / ロード機能 (UniTask) ---

        /// <summary>
        /// セーブデータが存在するか
        /// </summary>
        public bool HasSaveData()
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            return File.Exists(path);
        }

        /// <summary>
        /// 現在のゲーム状態を非同期でファイルに保存
        /// </summary>
        public async UniTask<bool> SaveGameAsync()
        {
            if (husband == null)
            {
                Debug.LogError("セーブ失敗: データが初期化されていません。");
                return false;
            }

            // セーブデータオブジェクトの作成
            GameSaveData saveData = new GameSaveData
            {
                // ここに作成したデータを保存していく
                PlayerData = this.husband.ToSaveData()
            };

            string json = JsonUtility.ToJson(saveData, prettyPrint: true);
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            await UniTask.RunOnThreadPool(() => File.WriteAllText(path, json));

            Debug.Log($"セーブ完了: {path}");
            return true;
        }

        /// <summary>
        /// セーブデータを非同期でロードし、ゲーム状態を復元します。
        /// </summary>
        public async UniTask<bool> LoadGameAsync()
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            if (!File.Exists(path))
            {
                Debug.LogWarning("セーブデータが見つかりません。");
                return false;
            }

            string json = await UniTask.RunOnThreadPool(() => File.ReadAllText(path));
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
            if (saveData == null)
            {
                Debug.LogError("データの読み込みに失敗しました（データ破損の可能性）。");
                return false;
            }

            // 各モデルの再構築・復元
            this.husband = new RuntimePlayerStatus(saveData.PlayerData);

            Debug.Log("ロード完了。ゲームを再開します。");
            return true;
        }
    }
}
