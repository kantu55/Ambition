using Ambition.DataStructures;
using Ambition.GameCore;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI.Meal
{
    public class MealSelectionPanel : MonoBehaviour
    {
        [Header("Main View")]
        [SerializeField] private MainGameView mainView;
        [Header("UI Components")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform menuButtonContainer;
        [SerializeField] private Button menuButtonPrefab;
        [SerializeField] private Button backButton;

        [Header("Menu Details Panel")]
        [SerializeField] private GameObject detailsPanelRoot;
        [SerializeField] private TextMeshProUGUI menuNameText;
        [SerializeField] private Image menuImage;
        [SerializeField] private TextMeshProUGUI menuEffectText;

        /// <summary>
        /// メニューが選択された時のコールバック
        /// </summary>
        public event Action<FoodMitModel> OnMenuSelected;

        /// <summary>
        /// 戻るボタンが押された時のコールバック
        /// </summary>
        public event Action OnBackPressed;

        private List<Button> instantiatedButtons = new List<Button>();
        private StringBuilder stringBuilder = new StringBuilder(256);
        private FoodMitModel currentlyDisplayedMenu;

        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackPressed);
            }

            // 初期状態では非表示
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (detailsPanelRoot != null)
            {
                detailsPanelRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HandleBackPressed);
            }
        }

        /// <summary>
        /// メニュー選択パネルを表示
        /// </summary>
        public void Show(List<FoodMitModel> menus)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            // 既存のボタンをクリア
            ClearButtons();

            // メニューボタンを生成
            foreach (var menu in menus)
            {
                CreateMenuButton(menu);
            }

            // 初期状態では詳細パネルは非表示
            if (detailsPanelRoot != null)
            {
                detailsPanelRoot.SetActive(false);
            }
        }

        /// <summary>
        /// メニュー選択パネルを非表示
        /// </summary>
        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (detailsPanelRoot != null)
            {
                detailsPanelRoot.SetActive(false);
            }

            ClearButtons();
        }

        /// <summary>
        /// メニューボタンを生成
        /// </summary>
        private void CreateMenuButton(FoodMitModel menu)
        {
            if (menuButtonPrefab == null || menuButtonContainer == null)
            {
                Debug.LogWarning("[MealSelectionPanel] menuButtonPrefab or menuButtonContainer is null");
                return;
            }

            Button button = Instantiate(menuButtonPrefab, menuButtonContainer);
            instantiatedButtons.Add(button);

            // ボタンのテキストを設定
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = menu.MenuName;
            }

            // クリックイベントを設定（メニュー詳細を表示）
            button.onClick.AddListener(() => HandleMenuHovered(menu));

            // ダブルクリックで確認画面へ（簡易実装：クリックで詳細表示、もう一度クリックで選択）
            // 実際のUIでは、詳細を表示した後に別の「選択」ボタンを押す形が一般的
        }

        /// <summary>
        /// メニューがクリックされた時の処理（詳細表示とメニュー選択を兼ねる）
        /// </summary>
        private void HandleMenuHovered(FoodMitModel menu)
        {
            // 詳細パネルを表示
            ShowMenuDetails(menu);

            // メニュー選択イベントを発火
            OnMenuSelected?.Invoke(menu);
        }

        /// <summary>
        /// メニュー詳細を表示
        /// </summary>
        private void ShowMenuDetails(FoodMitModel menu)
        {
            if (detailsPanelRoot != null)
            {
                detailsPanelRoot.SetActive(true);
            }

            currentlyDisplayedMenu = menu;

            // メニュー名
            if (menuNameText != null)
            {
                menuNameText.text = menu.MenuName;
            }

            // メニュー画像（現状はプレースホルダー）
            // 実際のUnityエディタでSprite参照を設定する必要がある
            if (menuImage != null)
            {
                // TODO: 画像の読み込み処理
                // menuImage.sprite = LoadMenuSprite(menu.MenuId);
            }

            // メニュー効果
            if (menuEffectText != null)
            {
                menuEffectText.text = BuildEffectText(menu);
            }

            if (mainView != null)
            {
                mainView.ShowPreview(menu.MitigHP, menu.MitigMP, menu.MitigCOND, 0, GameSimulationManager.Instance.Husband);
            }
        }

        /// <summary>
        /// メニュー効果のテキストを生成
        /// </summary>
        private string BuildEffectText(FoodMitModel menu)
        {
            stringBuilder.Clear();
            stringBuilder.Append("【効果】\n");

            if (menu.MitigHP != 0)
            {
                stringBuilder.Append($"体力回復: {FormatChangeValue(menu.MitigHP)}\n");
            }

            if (menu.MitigMP != 0)
            {
                stringBuilder.Append($"精神回復: {FormatChangeValue(menu.MitigMP)}\n");
            }

            if (menu.MitigCOND != 0)
            {
                stringBuilder.Append($"調子改善: {FormatChangeValue(menu.MitigCOND)}\n");
            }

            if (stringBuilder.Length == "【効果】\n".Length)
            {
                stringBuilder.Append("なし");
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 数値変化を表示用の文字列に変換（正の値には+を付ける）
        /// </summary>
        private string FormatChangeValue(int value)
        {
            if (value > 0)
            {
                return $"+{value}";
            }

            return value.ToString();
        }

        /// <summary>
        /// 戻るボタンが押された時の処理
        /// </summary>
        private void HandleBackPressed()
        {
            OnBackPressed?.Invoke();
            if (mainView != null)
            {
                mainView.HidePreview();
            }
        }

        /// <summary>
        /// 生成されたボタンをすべて削除
        /// </summary>
        private void ClearButtons()
        {
            foreach (var button in instantiatedButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            instantiatedButtons.Clear();
        }
    }
}
