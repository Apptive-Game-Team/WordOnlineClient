using System;

namespace Global
{
    /// <summary>
    /// One in-flight loading request held by <see cref="LoadingPage"/>.
    /// Release it with <see cref="Dispose"/>, or let the owner's destruction expire it.
    /// </summary>
    public sealed class LoadingHandle : IDisposable
    {
        /// Returned when no <see cref="LoadingPage"/> can serve the request. Disposing it is a no-op.
        internal static readonly LoadingHandle Detached = new LoadingHandle(null, null);

        private readonly UnityEngine.Object owner;
        private LoadingPage page;

        internal LoadingHandle(LoadingPage page, UnityEngine.Object owner)
        {
            this.page = page;
            this.owner = owner;
        }

        /// True once the owner is destroyed. Unity's overloaded equality reports destroyed objects as null.
        internal bool IsExpired => owner == null;

        public void Dispose()
        {
            if (page == null) return;
            page.Release(this);
            page = null;
        }
    }
}
