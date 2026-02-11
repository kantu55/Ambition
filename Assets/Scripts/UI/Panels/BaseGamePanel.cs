using System;
using UnityEngine;

namespace Ambition.UI.Panels
{
    /// <summary>
    /// UIパネルの基底クラス
    /// </summary>
    public abstract class BaseGamePanel : MonoBehaviour
    {
        /// <summary>
        /// パネルのルートオブジェクト
        /// </summary>
        [SerializeField] protected GameObject panelRoot;

        /// <summary>
        /// パネルが閉じられたときに発生するイベント
        /// </summary>
        public event Action OnPanelClosed;

        /// <summary>
        /// 表示状態
        /// </summary>
        public bool IsVisible { get; private set; }

        protected virtual void Awake()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            IsVisible = false;
        }

        public virtual void Show()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            IsVisible = true;
        }

        public virtual void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            IsVisible = false;
        }

        public virtual void Close()
        {
            Hide();
            OnPanelClosed?.Invoke();
        }

        // --- 拡張用メソッド ---
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
}
