using Ambition.DataStructures;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ambition.GameCore
{
    /// <summary>
    /// ゲーム起動時の初期化フローを管理するクラス
    /// </summary>
    public class GameBootController : MonoBehaviour
    {
        [SerializeField] private string nextSceneName = "TitleScene";

        private void Start()
        {
            InitializeFlowAsync().Forget();
        }

        /// <summary>
        /// 初期化処理の流れを制御
        /// </summary>
        private async UniTask InitializeFlowAsync()
        {
            await DataManager.Instance.LoadAllGameDataAsync();
            if (DataManager.Instance.GetDatas<PlayerStatsModel>().Count == 0)
            {
                Debug.LogWarning("選手データがロードされていません。");
            }

            await SceneManager.LoadSceneAsync(nextSceneName).ToUniTask();
        }
    }
}