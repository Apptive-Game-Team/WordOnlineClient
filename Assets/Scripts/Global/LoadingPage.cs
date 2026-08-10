using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Global
{
    /// <summary>
    /// Cross-scene loading overlay. Callers acquire a <see cref="LoadingHandle"/> with
    /// <see cref="Begin"/> and release it when their work finishes; the overlay is visible
    /// while at least one handle is alive.
    /// </summary>
    /// <remarks>
    /// Acquire from <c>Start</c> or later, never from <c>Awake</c>: a single-mode scene load
    /// releases every handle, and that reset runs after <c>Awake</c> but before <c>Start</c>.
    /// </remarks>
    public class LoadingPage : MonoBehaviour
    {
        [SerializeField] private GameObject loadingIndicator;

        private readonly List<LoadingHandle> activeHandles = new List<LoadingHandle>();

        public static LoadingPage Instance { get; private set; }

        public static bool IsLoading => Instance != null && Instance.activeHandles.Count > 0;

        /// <summary>
        /// Shows the overlay until the returned handle is disposed or <paramref name="owner"/> is destroyed.
        /// Never returns null, so callers can always wrap the result in <c>using</c>.
        /// </summary>
        public static LoadingHandle Begin(UnityEngine.Object owner)
        {
            if (Instance == null)
            {
                WDebug.LogWarning("LoadingPage is missing in this scene flow. Loading overlay is skipped.");
                return LoadingHandle.Detached;
            }

            return Instance.Acquire(owner);
        }

        /// Hides the overlay and drops every outstanding handle.
        public static void ReleaseAll()
        {
            if (Instance == null) return;
            if (Instance.activeHandles.Count == 0) return;

            Instance.activeHandles.Clear();
            Instance.ApplyIndicator();
        }

        internal void Release(LoadingHandle handle)
        {
            if (!activeHandles.Remove(handle)) return;
            ApplyIndicator();
        }

        private LoadingHandle Acquire(UnityEngine.Object owner)
        {
            if (owner == null)
            {
                WDebug.LogWarning("LoadingPage.Begin requires a live owner. Loading overlay is skipped.");
                return LoadingHandle.Detached;
            }

            LoadingHandle handle = new LoadingHandle(this, owner);
            activeHandles.Add(handle);
            ApplyIndicator();
            return handle;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance != this) return;

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            Instance = null;
        }

        private void Update()
        {
            ReleaseExpiredHandles();
        }

        /// <summary>
        /// A scene load destroys the MonoBehaviours that own in-flight handles, so any request
        /// that outlives the swap can never be released by its owner. No caller is allowed to
        /// keep the overlay across a scene boundary, so reset unconditionally.
        /// </summary>
        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Single) return;
            ReleaseAll();
        }

        /// Covers owners destroyed inside a scene, where no scene load reset ever fires.
        private void ReleaseExpiredHandles()
        {
            if (activeHandles.Count == 0) return;
            if (activeHandles.RemoveAll(handle => handle.IsExpired) == 0) return;

            ApplyIndicator();
        }

        private void ApplyIndicator()
        {
            bool shouldShow = activeHandles.Count > 0;
            if (loadingIndicator.activeSelf == shouldShow) return;

            loadingIndicator.SetActive(shouldShow);
            WDebug.Log($"Loading state changed: {shouldShow} (active requests: {activeHandles.Count})");
        }
    }
}
