using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System;
using Ambition.GameCore;
using Unity.VisualScripting;
using Ambition.DataStructures;

namespace ProBaseballSim.GameCore
{
    /// <summary>
    /// ゲーム起動時の初期化フローを管理するクラス
    /// </summary>
    public class GameBootController : MonoBehaviour
    {
        [SerializeField]
        private string nextSceneName = "TitleScene";

        private void Start()
        {
            InitializeFlowAsync().Forget();
        }

        /// <summary>
        /// 初期化処理の流れを制御
        /// </summary>
        private async UniTask InitializeFlowAsync()
        {
            Debug.Log("ゲーム初期化を開始します...");

            // 1. DataManagerの準備
            var dataManager = DataManager.Instance;

            // 2. データの非同期ロード実行
            // ここでロードが終わるまで完全に待機します
            await dataManager.LoadAllGameDataAsync();

            // データチェック
            if (dataManager.GetDatas<PlayerStatsModel>().Count == 0)
            {
                Debug.LogWarning("選手データがロードされていません。");
            }

            Debug.Log("データロード完了。1秒待機してから遷移します。");

            // 演出としての待機（UniTask.Delayを使用）
            await UniTask.Delay(1000); // 1000ミリ秒待機

            Debug.Log("タイトルシーンへ遷移します。");

            // 3. シーン遷移
            // LoadSceneAsync を await することで、遷移完了まで待機できます
            await SceneManager.LoadSceneAsync(nextSceneName).ToUniTask();
        }
    }
}