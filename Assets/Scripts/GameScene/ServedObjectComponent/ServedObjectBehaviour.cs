using UnityEngine;

namespace GameScene.ServedObjectComponent
{
    /// <summary>
    /// Base class for prefab components that react to <see cref="ServedObject"/> events.
    /// <para>
    /// Binding is explicit: <c>ObjectSpawner</c> walks the spawned hierarchy and calls
    /// <see cref="Bind"/> once the ServedObject is fully configured, so subscriptions never depend
    /// on Awake/Start ordering against the runtime <c>GetOrAddComponent&lt;ServedObject&gt;</c>.
    /// Objects created outside the spawner (the tutorial scene) still work through the Start
    /// fallback, which resolves the owner from the serialized field or the parent chain.
    /// </para>
    /// </summary>
    public abstract class ServedObjectBehaviour : MonoBehaviour, IServedObjectListener
    {
        [SerializeField] private ServedObject servedObject;

        private bool bound;

        /// <summary>The ServedObject this component reacts to. Null until bound.</summary>
        protected ServedObject Owner => servedObject;

        public void Bind(ServedObject target)
        {
            if (bound || target == null)
            {
                return;
            }

            servedObject = target;
            bound = true;
            OnBound();
        }

        private void Start()
        {
            if (bound)
            {
                return;
            }

            Bind(servedObject != null ? servedObject : GetComponentInParent<ServedObject>());
        }

        private void OnDestroy()
        {
            if (!bound)
            {
                return;
            }

            bound = false;
            OnUnbound();
        }

        /// <summary>Subscribe to <see cref="Owner"/> events here.</summary>
        protected virtual void OnBound()
        {
        }

        /// <summary>Unsubscribe here. <see cref="Owner"/> may already be destroyed.</summary>
        protected virtual void OnUnbound()
        {
        }

        /// <summary>
        /// Returns the serialized renderer, falling back to the first renderer under the owner's
        /// actual transform so a prefab that never wired the field still animates.
        /// </summary>
        protected SpriteRenderer ResolveRenderer(SpriteRenderer serializedRenderer)
        {
            if (serializedRenderer != null)
            {
                return serializedRenderer;
            }

            Transform actualTransform = Owner != null ? Owner.GetActualTransform() : null;
            return actualTransform != null ? actualTransform.GetComponentInChildren<SpriteRenderer>() : null;
        }
    }
}
