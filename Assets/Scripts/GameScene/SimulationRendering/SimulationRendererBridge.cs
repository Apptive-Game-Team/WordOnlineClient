using System;
using System.Collections.Generic;
using GameScene.Simulation.Core;
using GameScene.Simulation.Resources;
using UnityEngine;

namespace GameScene.Simulation.Rendering
{
    public sealed class SimulationRendererBridge : MonoBehaviour
    {
        [Serializable]
        public struct PrefabBinding
        {
            public string prefabId;
            public GameObject prefab;
        }

        private sealed class ViewState
        {
            public GameObject GameObject;
            public Vector3 Start;
            public Vector3 Target;
            public Quaternion StartRotation;
            public Quaternion TargetRotation;
            public string LastStatus;
            public long OwnerUserId;
        }

        // A stationary entity reports the same fixed-point position every frame, so this only has
        // to survive the fixed-point to float conversion, not real drift.
        private const float MovementEpsilonSqr = 1e-8f;

        [SerializeField] private PrefabBinding[] prefabBindings = Array.Empty<PrefabBinding>();
        [SerializeField, Min(0f)] private float interpolationDuration = 0.05f;
        [SerializeField] private MonoBehaviour playerUi;

        private readonly Dictionary<string, GameObject> prefabs =
            new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private readonly Dictionary<int, ViewState> views = new Dictionary<int, ViewState>();
        private float interpolationElapsed;
        private long leftUserId;
        private long rightUserId;

        public int BindingCount => views.Count;

        private void Awake() => RebuildPrefabRegistry();

        private void Update()
        {
            interpolationElapsed += Time.deltaTime;
            float alpha = interpolationDuration <= 0f
                ? 1f
                : Mathf.Clamp01(interpolationElapsed / interpolationDuration);
            foreach (ViewState view in views.Values)
            {
                if (view.GameObject == null) continue;
                view.GameObject.transform.position = Vector3.LerpUnclamped(view.Start, view.Target, alpha);
                view.GameObject.transform.rotation = Quaternion.SlerpUnclamped(
                    view.StartRotation, view.TargetRotation, alpha);
            }
        }

        private void OnDestroy() => ClearBindings();

        public void ApplySnapshot(SimulationSnapshot snapshot, PlayerResourceSnapshot playerSnapshot = null)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            interpolationElapsed = 0f;
            for (int index = 0; index < snapshot.Entities.Count; index++)
            {
                SimulationEntitySnapshot entity = snapshot.Entities[index];
                if (entity.IsDestroyed)
                {
                    RemoveDestroyed(entity);
                    continue;
                }
                if (ResolveResourcePrefabId(entity.PrefabId) == null)
                {
                    RemoveView(entity.Id);
                    continue;
                }
                bool spawned = !views.ContainsKey(entity.Id);
                ViewState view = GetOrCreate(entity);
                Vector3 previousTarget = view.Target;
                view.Start = view.GameObject.transform.position;
                view.Target = ToUnityPosition(entity.Position);
                view.StartRotation = view.GameObject.transform.rotation;
                view.TargetRotation = ToUnityRotation(entity.Orientation);
                ApplyPresentation(view, entity);
                NotifyMovement(view, spawned, previousTarget);
            }
            if (playerSnapshot != null)
            {
                if (!(playerUi is ISimulationPlayerUi ui))
                    throw new InvalidOperationException("Player UI must implement ISimulationPlayerUi");
                ui.Render(playerSnapshot);
            }
        }

        public GameObject FindView(int entityId)
        {
            return views.TryGetValue(entityId, out ViewState view) ? view.GameObject : null;
        }

        public void ConfigureForTests(PrefabBinding[] bindings, float duration = 0f)
        {
            prefabBindings = bindings ?? Array.Empty<PrefabBinding>();
            interpolationDuration = duration;
            RebuildPrefabRegistry();
        }

        public void ConfigurePlayerUi(MonoBehaviour ui)
        {
            if (!(ui is ISimulationPlayerUi))
                throw new ArgumentException("Player UI must implement ISimulationPlayerUi", nameof(ui));
            playerUi = ui;
        }

        public void ConfigurePlayerUiForTests(MonoBehaviour ui) => ConfigurePlayerUi(ui);

        public void ConfigureParticipants(long left, long right)
        { leftUserId = left; rightUserId = right; }

        public void ApplyLocalPlayerEffects(long userId, IReadOnlyList<string> effects)
        {
            foreach (ViewState view in views.Values)
            {
                if (view.OwnerUserId != userId || view.GameObject == null) continue;
                ISimulationEntityView entityView = FindEntityView(view.GameObject);
                if (entityView != null) entityView.ApplyLocalEffects(effects);
            }
        }

        public void ClearBindings()
        {
            foreach (ViewState view in views.Values)
                if (view.GameObject != null) Destroy(view.GameObject);
            views.Clear();
        }

        private ViewState GetOrCreate(SimulationEntitySnapshot entity)
        {
            if (views.TryGetValue(entity.Id, out ViewState existing)) return existing;
            string resourcePrefabId = ResolveResourcePrefabId(entity.PrefabId);
            if (resourcePrefabId == null)
                throw new InvalidOperationException("Simulation prefab has no renderer view: " + entity.PrefabId);
            if (!prefabs.TryGetValue(entity.PrefabId, out GameObject prefab) || prefab == null)
            {
                prefab = UnityEngine.Resources.Load<GameObject>("Prefabs/" + resourcePrefabId);
                if (prefab == null) throw new InvalidOperationException("Missing renderer prefab: " + entity.PrefabId);
                prefabs[entity.PrefabId] = prefab;
            }
            GameObject instance = Instantiate(prefab, ToUnityPosition(entity.Position), ToUnityRotation(entity.Orientation));
            instance.name = prefab.name + " [Simulation " + entity.Id + "]";
            SimulationViewBinding binding = instance.GetComponent<SimulationViewBinding>() ??
                                            instance.AddComponent<SimulationViewBinding>();
            binding.EntityId = entity.Id;
            ViewState created = new ViewState
            {
                GameObject = instance,
                Start = instance.transform.position,
                Target = instance.transform.position,
                StartRotation = instance.transform.rotation,
                TargetRotation = instance.transform.rotation,
                OwnerUserId = entity.OwnerUserId
            };
            views.Add(entity.Id, created);
            return created;
        }

        private void RemoveDestroyed(SimulationEntitySnapshot entity)
        {
            if (!views.TryGetValue(entity.Id, out ViewState view)) return;
            views.Remove(entity.Id);
            if (view.GameObject == null) return;
            ISimulationEntityView entityView = FindEntityView(view.GameObject);
            if (entityView == null)
            {
                Destroy(view.GameObject);
                return;
            }
            ApplyPresentation(view, entity);
        }

        private void RemoveView(int entityId)
        {
            if (!views.TryGetValue(entityId, out ViewState view)) return;
            views.Remove(entityId);
            if (view.GameObject != null) Destroy(view.GameObject);
        }

        private void ApplyPresentation(ViewState view, SimulationEntitySnapshot entity)
        {
            ISimulationEntityView entityView = FindEntityView(view.GameObject);
            if (entityView == null) return;
            List<string> effects = new List<string>(entity.Effects.Count);
            for (int index = 0; index < entity.Effects.Count; index++) effects.Add(entity.Effects[index]);
            string status = string.Equals(view.LastStatus, entity.Status, StringComparison.Ordinal)
                ? null : entity.Status;
            view.LastStatus = entity.Status;
            entityView.ApplySimulationState(status, effects, entity.Gauges, Master(entity.OwnerUserId));
        }

        /// <summary>
        /// Drives the prefab presentation contract main's ObjectSpawner used to own. Prefab
        /// components subscribe through <c>IServedObjectListener</c>, so without the bind every
        /// sound, spawn animation, and motion component on a spawned prefab stays inert.
        /// </summary>
        private static void NotifyMovement(ViewState view, bool spawned, Vector3 previousTarget)
        {
            ISimulationEntityView entityView = FindEntityView(view.GameObject);
            if (entityView == null) return;
            if (spawned)
            {
                entityView.BindListeners();
                entityView.NotifySpawned();
                return;
            }
            if ((view.Target - previousTarget).sqrMagnitude > MovementEpsilonSqr) entityView.NotifyMoved();
        }

        private static ISimulationEntityView FindEntityView(GameObject gameObject) =>
            gameObject == null ? null : gameObject.GetComponent(typeof(ISimulationEntityView)) as ISimulationEntityView;

        private string Master(long ownerUserId)
        {
            if (ownerUserId == leftUserId) return "LeftPlayer";
            if (ownerUserId == rightUserId) return "RightPlayer";
            return "None";
        }

        private void RebuildPrefabRegistry()
        {
            prefabs.Clear();
            for (int index = 0; index < prefabBindings.Length; index++)
            {
                PrefabBinding binding = prefabBindings[index];
                if (string.IsNullOrWhiteSpace(binding.prefabId) || binding.prefab == null)
                    throw new InvalidOperationException("Renderer prefab binding is incomplete at index " + index);
                if (prefabs.ContainsKey(binding.prefabId))
                    throw new InvalidOperationException("Duplicate renderer prefab binding: " + binding.prefabId);
                prefabs.Add(binding.prefabId, binding.prefab);
            }
        }

        private static Vector3 ToUnityPosition(SimVector3 value) =>
            new Vector3((float)value.X, (float)value.Z, (float)value.Y);

        private static Quaternion ToUnityRotation(SimQuaternion value) =>
            new Quaternion((float)value.X, (float)value.Z, (float)value.Y, (float)value.W);

        public static string ResolveResourcePrefabId(string simulationPrefabId)
        {
            switch (simulationPrefabId)
            {
                case "FireSummon": return "FireSlime";
                case "ElectricSummon": return "ElectricSlime";
                case "RockSummon": return "RockSlime";
                case "WindSummon": return "WindSlime";
                case "VineToss": return "Vine";
                case "Wall": return null;
                default: return simulationPrefabId;
            }
        }
    }
}
