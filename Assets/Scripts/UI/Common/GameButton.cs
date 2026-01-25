using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Ambition.UI.Common
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

        [SerializeField]
        private Image targetImage;

        [Header("Animation Settings")]
        [SerializeField]
        private float animationDuration = 0.1f;

        [SerializeField]
        private float hoverScale = 1.1f;

        [SerializeField]
        private float pressedScale = 0.95f;

        [SerializeField]
        private Ease animEase = Ease.OutQuad;

        [Header("Color Settings")]
        [SerializeField]
        private bool enableColorChange = true;

        [SerializeField]
        private Color selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        // 元のスケールと色を保持
        private Vector3 defaultScale;
        private Color defaultColor;

        private void Awake()
        {
            defaultScale = transform.localScale;
            if (targetButton == null)
            {
                targetButton = GetComponent<Button>();
            }

            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }

            if (targetImage != null)
            {
                defaultColor = targetImage.color;
            }
        }

        private void OnDisable()
        {
            transform.DOKill();
            transform.localScale = defaultScale;

            if (targetImage != null)
            {
                targetImage.DOKill();
                targetImage.color = defaultColor;
            }
        }

        // --- インターフェース実装 ---

        /// <summary>
        /// マウスカーソルが乗った時に呼ばれる
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (IsButtonInteractable())
            {
                targetButton.Select();
            }

            PlayScaleAnimation(hoverScale, selectedColor);
        }

        /// <summary>
        /// マウスカーソルが離れた時に呼ばれる
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            PlayScaleAnimation(1.0f, defaultColor);
        }

        /// <summary>
        /// キーボードやコントローラーで選択された時に呼ばれる
        /// </summary>
        public void OnSelect(BaseEventData eventData)
        {
            PlayScaleAnimation(hoverScale, selectedColor);
        }

        /// <summary>
        /// キーボードやコントローラーの選択が外れた時に呼ばれる
        /// </summary>
        public void OnDeselect(BaseEventData eventData)
        {
            PlayScaleAnimation(1.0f, defaultColor);
        }

        /// <summary>
        /// ボタンが押下された瞬間（クリック/決定ボタン）に呼ばれる
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsButtonInteractable())
            {
                PlayScaleAnimation(pressedScale, selectedColor);
            }
        }

        /// <summary>
        /// ボタンの押下が解除された瞬間に呼ばれる
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (IsButtonInteractable())
            {
                PlayScaleAnimation(hoverScale, selectedColor);
            }
        }

        // --- 内部ロジック ---

        /// <summary>
        /// 指定したスケールと色へ滑らかに変化させる
        /// </summary>
        private void PlayScaleAnimation(float targetScaleMultiplier, Color targetColor)
        {
            transform.DOKill();
            Vector3 endValue = defaultScale * targetScaleMultiplier;
            transform.DOScale(endValue, animationDuration).SetEase(animEase).SetLink(gameObject).SetUpdate(true);

            if (targetImage != null && enableColorChange)
            {
                targetImage.DOKill();
                targetImage.DOColor(targetColor, animationDuration).SetEase(animEase).SetLink(gameObject).SetUpdate(true);
            }
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