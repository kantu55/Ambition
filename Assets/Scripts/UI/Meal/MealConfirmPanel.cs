using Ambition.Data.Master;
using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ambition.UI.Meal
{
    /// <summary>
    /// ③食事_確認ダイアログ のパネル
    /// 選択されたメニューの詳細と確認ボタンを表示
    /// </summary>
    public class MealConfirmPanel : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI confirmText;
        [SerializeField] private TextMeshProUGUI menuNameText;
        [SerializeField] private Image menuImage;
        [SerializeField] private TextMeshProUGUI menuEffectText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button backButton;

        /// <summary>
        /// 確認ボタンが押された時のコールバック
        /// </summary>
        public event Action<FoodMitModel> OnConfirmPressed;

        /// <summary>
        /// 戻るボタンが押された時のコールバック
        /// </summary>
        public event Action OnBackPressed;

        private FoodMitModel currentMenu;
        private StringBuilder stringBuilder = new StringBuilder(256);

        private void Awake()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(HandleConfirmPressed);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackPressed);
            }

            // 初期状態では非表示
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(HandleConfirmPressed);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HandleBackPressed);
            }
        }

        /// <summary>
        /// 確認ダイアログを表示
        /// </summary>
        public void Show(FoodMitModel menu)
        {
            if (menu == null)
            {
                Debug.LogWarning("[MealConfirmPanel] menu is null");
                return;
            }

            currentMenu = menu;

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            UpdateContent();
        }

        /// <summary>
        /// 確認ダイアログを非表示
        /// </summary>
        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            currentMenu = null;
        }

        /// <summary>
        /// ダイアログの内容を更新
        /// </summary>
        private void UpdateContent()
        {
            if (currentMenu == null)
            {
                return;
            }

            // 確認テキスト
            if (confirmText != null)
            {
                confirmText.text = "この食事を選択しますか？";
            }

            // メニュー名
            if (menuNameText != null)
            {
                menuNameText.text = currentMenu.MenuName;
            }

            // メニュー画像（現状はプレースホルダー）
            if (menuImage != null)
            {
                // TODO: 画像の読み込み処理
                // menuImage.sprite = LoadMenuSprite(currentMenu.MenuId);
            }

            // メニュー効果
            if (menuEffectText != null)
            {
                menuEffectText.text = BuildEffectText(currentMenu);
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
        /// 確認ボタンが押された時の処理
        /// </summary>
        private void HandleConfirmPressed()
        {
            if (currentMenu != null)
            {
                OnConfirmPressed?.Invoke(currentMenu);
            }
        }

        /// <summary>
        /// 戻るボタンが押された時の処理
        /// </summary>
        private void HandleBackPressed()
        {
            OnBackPressed?.Invoke();
        }
    }
}
