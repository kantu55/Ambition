using System;
using UnityEngine;

namespace Ambition.UI.Panels
{
    /// <summary>
    /// Abstract base class for game UI panels.
    /// Provides common functionality for showing/hiding panels and handling user interactions.
    /// </summary>
    public abstract class BaseGamePanel : MonoBehaviour
    {
        [Header("Base Panel Settings")]
        [SerializeField] protected GameObject panelRoot;

        /// <summary>
        /// Event fired when the panel is closed
        /// </summary>
        public event Action OnPanelClosed;

        /// <summary>
        /// Whether the panel is currently visible
        /// </summary>
        public bool IsVisible { get; protected set; }

        protected virtual void Awake()
        {
            // Ensure panel is hidden by default
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
            IsVisible = false;
        }

        /// <summary>
        /// Show the panel
        /// </summary>
        public virtual void Show()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
            IsVisible = true;
            OnShow();
        }

        /// <summary>
        /// Hide the panel
        /// </summary>
        public virtual void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
            IsVisible = false;
            OnHide();
        }

        /// <summary>
        /// Close the panel and notify listeners
        /// </summary>
        public virtual void Close()
        {
            Hide();
            OnPanelClosed?.Invoke();
        }

        /// <summary>
        /// Called when the panel is shown. Override for custom behavior.
        /// </summary>
        protected virtual void OnShow()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Called when the panel is hidden. Override for custom behavior.
        /// </summary>
        protected virtual void OnHide()
        {
            // Override in derived classes
        }
    }
}
