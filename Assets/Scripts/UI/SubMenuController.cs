using Ambition.DataStructures;

namespace Ambition.UI
{
    /// <summary>
    /// サブメニュー（行動リスト）を表示・管理するコントローラー
    /// </summary>
    public class SubMenuController : DetailPanelController<WifeActionModel>
    {
        [UnityEngine.SerializeField] private MainGameView mainGameView;

        /// <summary>
        /// 行動が選択された時のコールバック（互換性のため）
        /// </summary>
        public event System.Action<WifeActionModel> OnActionSelected;

        /// <summary>
        /// 行動が確定された時のコールバック（互換性のため）
        /// </summary>
        public event System.Action<WifeActionModel> OnActionConfirmed;

        protected override void Awake()
        {
            base.Awake();

            // 基底クラスのイベントを旧イベントにブリッジ
            OnItemSelected += (action) => OnActionSelected?.Invoke(action);
            OnItemConfirmed += (action) => OnActionConfirmed?.Invoke(action);
        }

        /// <summary>
        /// 行動を選択して詳細パネルを更新（互換性のため）
        /// </summary>
        public void SelectAction(WifeActionModel action)
        {
            SelectItem(action);
        }

        // === 基底クラスの抽象メソッド実装 ===

        protected override string GetItemName(WifeActionModel action)
        {
            return action.Name;
        }

        protected override string BuildCostText(WifeActionModel action)
        {
            stringBuilder.Clear();
            stringBuilder.Append(COST_HEADER);

            if (action.CashCost != 0)
            {
                stringBuilder.Append($"資金: {action.CashCost:N0}{CURRENCY_UNIT}\n");
            }

            if (stringBuilder.Length == COST_HEADER.Length)
            {
                stringBuilder.Append(NONE_TEXT);
            }

            return stringBuilder.ToString();
        }

        protected override string BuildEffectsText(WifeActionModel action)
        {
            stringBuilder.Clear();
            stringBuilder.Append(EFFECTS_HEADER);

            // 夫への効果
            if (action.DeltaHP != 0)
            {
                stringBuilder.Append($"夫体力: {FormatChangeValue(action.DeltaHP)}\n");
            }

            if (action.DeltaMP != 0)
            {
                stringBuilder.Append($"夫精神: {FormatChangeValue(action.DeltaMP)}\n");
            }

            if (action.DeltaCOND != 0)
            {
                stringBuilder.Append($"夫調子: {FormatChangeValue(action.DeltaCOND)}\n");
            }

            if (action.DeltaLove != 0)
            {
                stringBuilder.Append($"愛情: {FormatChangeValue(action.DeltaLove)}\n");
            }

            if (action.DeltaPublicEye != 0)
            {
                stringBuilder.Append($"世間の目: {FormatChangeValue(action.DeltaPublicEye)}\n");
            }

            if (action.DeltaTeamEvaluation != 0)
            {
                stringBuilder.Append($"チーム評価: {FormatChangeValue(action.DeltaTeamEvaluation)}\n");
            }

            if (stringBuilder.Length == EFFECTS_HEADER.Length)
            {
                stringBuilder.Append(NONE_TEXT);
            }

            return stringBuilder.ToString();
        }

        protected override void UpdateItemImage(WifeActionModel action)
        {
            // 画像（将来的に実装される場合のため）
            // if (detailImage != null)
            // {
            //     detailImage.sprite = action.Image;
            // }
        }

        protected override void ShowPreview(WifeActionModel action)
        {
            // プレビュー表示を更新
            if (mainGameView != null)
            {
                mainGameView.ShowPreview(action.DeltaHP, action.DeltaMP, action.DeltaCOND, action.DeltaTeamEvaluation, action.DeltaLove, action.DeltaPublicEye);
            }
        }
    }
}
