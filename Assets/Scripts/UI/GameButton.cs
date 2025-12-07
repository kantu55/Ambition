using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Assets.Scripts.UI
{
    /// <summary>
    /// マウス、キーボード、コントローラー入力に対応した汎用UIボタン
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class GameButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, // マウス用
        ISelectHandler, IDeselectHandler,          // キーボード・コントローラー用
        IPointerDownHandler, IPointerUpHandler
    {
        [Header("Component")]
        [SerializeField]
        private Button targetButton;

        [Header("Animation Settings")]
        [SerializeField]
        private float animationDuration = 0.1f;

        [SerializeField]
        private float hoverScale = 1.1f;

        [SerializeField]
        private float pressedScale = 0.95f;

        [SerializeField]
        private Ease scaleEase = Ease.OutQuad;

        // 元のスケールを保持
        private Vector3 defaultScale;

        private void Awake()
        {
            defaultScale = transform.localScale;
            if (targetButton == null)
            {
                targetButton = GetComponent<Button>();
            }
        }

        private void OnDisable()
        {
            transform.DOKill();
            transform.localScale = defaultScale;
        }

        // --- インターフェース実装 ---

        /// <summary>
        /// マウスカーソルが乗った時に呼ばれる
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (IsButtonInteractable())
            {
                eventData.selectedObject = gameObject;
            }

            PlayScaleAnimation(hoverScale);
        }

        /// <summary>
        /// マウスカーソルが離れた時に呼ばれる
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            PlayScaleAnimation(1.0f);
        }

        /// <summary>
        /// キーボードやコントローラーで選択された時に呼ばれる
        /// </summary>
        public void OnSelect(BaseEventData eventData)
        {
            PlayScaleAnimation(hoverScale);
        }

        /// <summary>
        /// キーボードやコントローラーの選択が外れた時に呼ばれる
        /// </summary>
        public void OnDeselect(BaseEventData eventData)
        {
            PlayScaleAnimation(1.0f);
        }

        /// <summary>
        /// ボタンが押下された瞬間（クリック/決定ボタン）に呼ばれる
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsButtonInteractable())
            {
                PlayScaleAnimation(pressedScale);
            }
        }

        /// <summary>
        /// ボタンの押下が解除された瞬間に呼ばれる
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (IsButtonInteractable())
            {
                PlayScaleAnimation(hoverScale);
            }
        }

        // --- 内部ロジック ---

        /// <summary>
        /// 指定したスケールへ滑らかに変化させる
        /// </summary>
        private void PlayScaleAnimation(float targetScaleMultiplier)
        {
            transform.DOKill();
            Vector3 endValue = defaultScale * targetScaleMultiplier;
            transform.DOScale(endValue, animationDuration).SetEase(scaleEase).SetLink(gameObject).SetUpdate(true);
        }

        /// <summary>
        /// 操作可能か
        /// </summary>
        private bool IsButtonInteractable()
        {
            return targetButton != null && targetButton.interactable;
        }
    }
}