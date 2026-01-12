using Ambition.DataStructures;

namespace Ambition.UI.Meal
{
    /// <summary>
    /// 食事選択メニューコントローラー
    /// 詳細パネル付きの汎用コントローラーを継承
    /// </summary>
    public class MealDetailPanelController : DetailPanelController<FoodMitModel>
    {
        [UnityEngine.SerializeField] private MainGameView mainGameView;

        /// <summary>
        /// メニューが選択された時のコールバック（互換性のため）
        /// </summary>
        public event System.Action<FoodMitModel> OnMenuSelected;

        /// <summary>
        /// メニューが確定された時のコールバック（互換性のため）
        /// </summary>
        public event System.Action<FoodMitModel> OnMenuConfirmed;

        protected override void Awake()
        {
            base.Awake();

            // 基底クラスのイベントを旧イベントにブリッジ
            OnItemSelected += (menu) => OnMenuSelected?.Invoke(menu);
            OnItemConfirmed += (menu) => OnMenuConfirmed?.Invoke(menu);
        }

        /// <summary>
        /// メニューを選択して詳細パネルを更新（互換性のため）
        /// </summary>
        public void SelectMenu(FoodMitModel menu)
        {
            SelectItem(menu);
        }

        /// <summary>
        /// パネルを表示（互換性のため）
        /// </summary>
        public void Show(System.Collections.Generic.List<FoodMitModel> menus)
        {
            Open(menus);
        }

        /// <summary>
        /// パネルを非表示（互換性のため）
        /// </summary>
        public void Hide()
        {
            Close();
        }

        // === 基底クラスの抽象メソッド実装 ===

        protected override string GetItemName(FoodMitModel menu)
        {
            return menu.MenuName;
        }

        protected override string BuildCostText(FoodMitModel menu)
        {
            // 食事にはコストがないため空を返す
            return string.Empty;
        }

        protected override string BuildEffectsText(FoodMitModel menu)
        {
            stringBuilder.Clear();
            stringBuilder.Append(EFFECTS_HEADER);

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

            if (stringBuilder.Length == EFFECTS_HEADER.Length)
            {
                stringBuilder.Append(NONE_TEXT);
            }

            return stringBuilder.ToString();
        }

        protected override void UpdateItemImage(FoodMitModel menu)
        {
            // 画像（将来的に実装される場合のため）
            // if (detailImage != null)
            // {
            //     detailImage.sprite = LoadMenuSprite(menu.MenuId);
            // }
        }

        protected override void ShowPreview(FoodMitModel menu)
        {
            // プレビュー表示を更新
            if (mainGameView != null)
            {
                mainGameView.ShowPreview(menu.MitigHP, menu.MitigMP, menu.MitigCOND);
            }
        }

        protected override void HandleBackPressed()
        {
            base.HandleBackPressed();
            
            // プレビューを非表示
            if (mainGameView != null)
            {
                mainGameView.HidePreview();
            }
        }
    }
}
