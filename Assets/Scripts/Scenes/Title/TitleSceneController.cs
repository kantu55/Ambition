using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

namespace ProBaseballSim.Scenes.Title
{
    /// <summary>
    /// タイトル画面のUI操作とシーン遷移を制御するクラス
    /// </summary>
    public class TitleSceneController : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField]
        private string nextSceneName = "MainGameScene";

        [Header("UI References")]
        [SerializeField]
        private Button startButton;

        [SerializeField]
        private Button exitButton;

        private void Start()
        {
            SetupButtons();

            if (startButton != null)
            {
                EventSystem.current.SetSelectedGameObject(startButton.gameObject);
            }
        }

        private void SetupButtons()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(() => OnStartClicked().Forget());
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(() => OnExitClicked().Forget());
            }
        }

        /// <summary>
        /// ゲーム開始ボタン押下時の処理。
        /// </summary>
        private async UniTaskVoid OnStartClicked()
        {
            Debug.Log("ゲーム開始ボタンが押されました。");

            startButton.interactable = false;
            exitButton.interactable = false;

            // ボタンの「押し込みアニメーション」や「SE」が終わるのを少し待つ演出
            await UniTask.Delay(300);

            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                await SceneManager.LoadSceneAsync(nextSceneName).ToUniTask();
            }
            else
            {
                Debug.LogError($"シーン '{nextSceneName}' が見つかりません。Build Settingsを確認してください。");

                // エラー時は操作不能にならないよう復帰させる
                startButton.interactable = true;
                exitButton.interactable = true;
            }
        }

        /// <summary>
        /// ゲーム終了ボタン押下時の処理。
        /// </summary>
        private async UniTaskVoid OnExitClicked()
        {
            Debug.Log("ゲーム終了ボタンが押されました。");

            startButton.interactable = false;
            exitButton.interactable = false;

            await UniTask.Delay(300);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
