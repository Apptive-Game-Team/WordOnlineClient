using System;
using System.Collections;
using System.Collections.Generic;
using GameScene.Simulation.Core;
using GameScene.Simulation.Rendering;
using Data;
using Global;
using UnityEngine;

namespace GameScene.ServedObjectComponent
{
    public class ServedObject : MonoBehaviour, ISimulationEntityView
    {
        private const string LeftPlayer = "LeftPlayer";
        private const string RightPlayer = "RightPlayer";
        private const string TeamIndicatorResourcePath = "UI/ObjectIndicator";
        private static readonly Color LeftIndicatorColor = new Color(0.92f, 0.24f, 0.24f, 1f);
        private static readonly Color RightIndicatorColor = new Color(0.25f, 0.55f, 0.98f, 1f);
        private static Sprite _teamIndicatorSprite;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Transform _actualTransform = null;
        [SerializeField] private float _teamIndicatorVerticalOffset = 0.1f;
        [SerializeField] private float _teamIndicatorScale = 0.3f;
        [SerializeField] private float _effectScaleReferenceHeight = 1.2f;
        [SerializeField] private float _effectScaleMultiplier = 1f;
        [SerializeField] private float _effectScaleMin = 0.8f;
        [SerializeField] private float _effectScaleMax = 1.8f;
        public int id;
        
        public List<Gauge> gauges = new List<Gauge>();
        
        private string master;
        private Transform _teamIndicatorTransform;
        private SpriteRenderer _teamIndicatorRenderer;
        private ServedObjectEffectRenderer _effectRenderer;
        private readonly List<string> _simulationEffects = new List<string>();
        private readonly List<string> _localEffects = new List<string>();
        private ServedObjectGaugeBar _teamColorGaugeBar;

        public event Action OnAttack;
        public event Action OnDamaged;
        public event Action<string> OnOtherStatus;
        public event Action OnDestroyed;
        public event Action OnMoved;
        public event Action<Gauge> OnGaugeChanged;
        
        public event Action OnHpIncreased;
        public event Action OnHpDecreased;

        /// <summary>
        /// Raised once, right after spawning, only when the spawn presentation should play. Objects
        /// that appear through a full-state sync rather than a real spawn never raise it.
        /// </summary>
        public event Action OnSpawned;

        private int lastHp = 0;
        private bool hasReceivedHp;
        
        public void SetMaster(string master)
        {
            this.master = master;

            EnsureSpriteRenderer();
            if (_spriteRenderer == null)
            {
                return;
            }

            UpdateTeamIndicator();

            if (master.Equals(RightPlayer))
            {
                if (transform.rotation.eulerAngles.y == 0)
                {
                    _spriteRenderer.flipX = true;
                    return;
                }
                gameObject.transform.Rotate(0, 180, 0);
            }
        }

        /// <summary>
        /// Hands this ServedObject to every <see cref="IServedObjectListener"/> in the hierarchy.
        /// Call once, after the object is fully configured, so prefab components can subscribe
        /// without depending on Awake/Start ordering.
        /// </summary>
        public void BindListeners()
        {
            IServedObjectListener[] listeners = GetComponentsInChildren<IServedObjectListener>(true);
            foreach (IServedObjectListener listener in listeners)
            {
                listener.Bind(this);
            }
        }

        /// <summary>Raises <see cref="OnSpawned"/>. Only the spawner should call this.</summary>
        public void NotifySpawned()
        {
            OnSpawned?.Invoke();
        }

        /// <summary>
        /// Raises <see cref="OnMoved"/>. The renderer owns the transform, so it also owns the
        /// decision that this object moved. Only the spawner should call this.
        /// </summary>
        public void NotifyMoved()
        {
            OnMoved?.Invoke();
        }

        private void OnEnable()
        {
            OnGaugeChanged += HandleDamageEffect;
        }

        private void OnDisable()
        {
            OnGaugeChanged -= HandleDamageEffect;
        }

        private void LateUpdate()
        {
            UpdateTeamIndicatorPosition();
        }
        
        public string GetMaster()
        {
            return master;
        }

        public void ApplySimulationState(string status, List<string> effects,
            List<Gauge> updatedGauges, string updatedMaster)
        {
            UpdateMasterIfNeeded(updatedMaster);
            EnsureEffectRenderer();
            _simulationEffects.Clear();
            if (effects != null) _simulationEffects.AddRange(effects);
            RenderCombinedEffects();
            HandleGaugeUpdate(updatedGauges);
            if (!string.IsNullOrEmpty(status)) HandleStatus(status);
        }

        void ISimulationEntityView.ApplySimulationState(string status,
            IReadOnlyList<string> effects, IReadOnlyList<SimulationGaugeSnapshot> gauges,
            string updatedMaster)
        {
            List<string> effectList = effects == null
                ? null : new List<string>(effects);
            List<Gauge> gaugeList = new List<Gauge>(gauges?.Count ?? 0);
            if (gauges != null)
                for (int index = 0; index < gauges.Count; index++)
                {
                    SimulationGaugeSnapshot gauge = gauges[index];
                    gaugeList.Add(new Gauge
                    {
                        value = gauge.Value,
                        maxValue = gauge.MaxValue,
                        category = gauge.Category
                    });
                }
            ApplySimulationState(status, effectList, gaugeList, updatedMaster);
        }

        public void ApplyLocalEffects(IReadOnlyList<string> effects)
        {
            _localEffects.Clear();
            if (effects != null)
                for (int index = 0; index < effects.Count; index++)
                    if (!_localEffects.Contains(effects[index])) _localEffects.Add(effects[index]);
            EnsureEffectRenderer();
            RenderCombinedEffects();
        }

        private void RenderCombinedEffects()
        {
            List<string> combined = new List<string>(_simulationEffects);
            for (int index = 0; index < _localEffects.Count; index++)
                if (!combined.Contains(_localEffects[index])) combined.Add(_localEffects[index]);
            _effectRenderer.SetEffects(combined);
        }
        
        private void HandleStatus(string status)
        {
            switch (status)
            {
                case "Destroyed":
                    DestroySelf(GameConfig.FRAME_DURATION);
                    break;

                case "Attack":
                    PlayAttackPresentation();
                    break;

                case "Damaged":
                    OnDamaged?.Invoke();
                    break;

                default:
                    OnOtherStatus?.Invoke(status);
                    break;
            }
        }

        public void PlayAttackPresentation()
        {
            OnAttack?.Invoke();
            DOTweenAction.SwingMobAttack(GetActualTransform());
        }
        
        public Transform GetActualTransform()
        {
            if (_actualTransform != null)
            {
                return _actualTransform;
            }

            Transform namedActualTransform = transform.Find("actualObject");
            if (namedActualTransform != null)
            {
                _actualTransform = namedActualTransform;
                return _actualTransform;
            }

            EnsureSpriteRenderer();
            if (_spriteRenderer != null)
            {
                _actualTransform = _spriteRenderer.transform;
                return _actualTransform;
            }
            
            _actualTransform = transform;
            return _actualTransform;
        }

        public Vector3 GetSpeechBubbleAnchorWorldPosition(float verticalOffset = 0.15f)
        {
            EnsureSpriteRenderer();
            Vector3 anchorUp = GetAnchorUpDirection();

            if (_spriteRenderer != null && _spriteRenderer.sprite != null)
            {
                // Sprites are billboarded to the tilted camera in 2.5D, so read the top in the
                // renderer's own space. A world AABB loses the depth the tilt adds and drops the
                // anchor onto the sprite itself.
                Bounds localBounds = _spriteRenderer.sprite.bounds;
                Vector3 topWorldPosition = _spriteRenderer.transform.TransformPoint(
                    new Vector3(localBounds.center.x, localBounds.max.y, 0f));
                return topWorldPosition + anchorUp * verticalOffset;
            }

            return GetActualTransform().position + anchorUp * (1f + verticalOffset);
        }

        /// <summary>Screen-up in world space, so anchors sit above the sprite from the player's view.</summary>
        private static Vector3 GetAnchorUpDirection()
        {
            Camera camera = Camera.main;
            return camera != null ? camera.transform.up : Vector3.up;
        }

        private void EnsureEffectRenderer()
        {
            if (_effectRenderer != null)
            {
                return;
            }

            _effectRenderer = new ServedObjectEffectRenderer(
                GetActualTransform,
                GetSpriteWorldHeight,
                _effectScaleReferenceHeight,
                _effectScaleMultiplier,
                _effectScaleMin,
                _effectScaleMax);
        }

        private void HandleGaugeUpdate(List<Gauge> gauges)
        {
            foreach (Gauge gauge in gauges)
            {
                Gauge temp = this.gauges.Find(existedGauge => existedGauge.category.Equals(gauge.category));
                if (temp == null)
                {
                    this.gauges.Add(gauge);
                }
                else
                {
                    temp.maxValue = gauge.maxValue;
                    temp.value = gauge.value;
                }
                OnGaugeChanged?.Invoke(gauge);
            }
        }
        
        private void HandleDamageEffect(Gauge gauge)
        {
            if (!gauge.category.Equals("HP")) return;

            if (!hasReceivedHp)
            {
                lastHp = (int) gauge.value;
                hasReceivedHp = true;
                return;
            }
            
            if (gauge.value < lastHp)
            {
                OnHpDecreased?.Invoke();
            }
            if (gauge.value > lastHp && !Mathf.Approximately(gauge.value, gauge.maxValue))
            {
                OnHpIncreased?.Invoke();
            }
            lastHp = (int) gauge.value;
        }

        private void UpdateMasterIfNeeded(string updatedMaster)
        {
            if (string.IsNullOrEmpty(updatedMaster) || string.Equals(master, updatedMaster, StringComparison.Ordinal))
            {
                return;
            }

            master = updatedMaster;
            UpdateTeamIndicator();
        }

        private void EnsureSpriteRenderer()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private float GetSpriteWorldHeight()
        {
            EnsureSpriteRenderer();
            if (_spriteRenderer == null)
            {
                return 0f;
            }

            return Mathf.Max(_spriteRenderer.bounds.size.y, _spriteRenderer.bounds.size.x);
        }

        private void UpdateTeamIndicator()
        {
            if (TryUpdateGaugeBarTeamIndicator())
            {
                DisableRuntimeTeamIndicator();
                return;
            }

            EnsureTeamIndicator();
            if (_teamIndicatorRenderer == null)
            {
                return;
            }

            if (!TryGetIndicatorColor(master, out Color indicatorColor))
            {
                _teamIndicatorRenderer.enabled = false;
                return;
            }

            _teamIndicatorRenderer.enabled = true;
            _teamIndicatorRenderer.color = indicatorColor;
            UpdateTeamIndicatorSorting();
            UpdateTeamIndicatorPosition();
        }

        private bool TryUpdateGaugeBarTeamIndicator()
        {
            if (_teamColorGaugeBar == null)
            {
                // Buildings carry a TTL bar built from the same component, so pick by role
                // rather than by hierarchy order — only the HP bar owns the team indicator.
                foreach (ServedObjectGaugeBar bar in GetComponentsInChildren<ServedObjectGaugeBar>(true))
                {
                    if (bar.UsesTeamColors)
                    {
                        _teamColorGaugeBar = bar;
                        break;
                    }
                }
            }

            if (_teamColorGaugeBar == null)
            {
                return false;
            }

            _teamColorGaugeBar.SetObjectIndicatorMaster(master);
            return true;
        }

        private void DisableRuntimeTeamIndicator()
        {
            if (_teamIndicatorRenderer == null)
            {
                return;
            }

            _teamIndicatorRenderer.enabled = false;
        }

        private void EnsureTeamIndicator()
        {
            if (_teamIndicatorRenderer != null)
            {
                return;
            }

            GameObject indicatorObject = new GameObject("TeamIndicator");
            _teamIndicatorTransform = indicatorObject.transform;
            _teamIndicatorTransform.SetParent(transform, false);
            _teamIndicatorTransform.localScale = Vector3.one * _teamIndicatorScale;

            _teamIndicatorRenderer = indicatorObject.AddComponent<SpriteRenderer>();
            _teamIndicatorRenderer.sprite = GetTeamIndicatorSprite();
            _teamIndicatorRenderer.enabled = false;
        }

        private void UpdateTeamIndicatorPosition()
        {
            if (_teamIndicatorRenderer == null || !_teamIndicatorRenderer.enabled)
            {
                return;
            }

            if (_teamIndicatorTransform == null)
            {
                _teamIndicatorTransform = _teamIndicatorRenderer.transform;
            }

            _teamIndicatorTransform.position = GetTeamIndicatorWorldPosition();
            UpdateTeamIndicatorSorting();
        }

        private Vector3 GetTeamIndicatorWorldPosition()
        {
            return GetSpeechBubbleAnchorWorldPosition(_teamIndicatorVerticalOffset);
        }

        private void UpdateTeamIndicatorSorting()
        {
            EnsureSpriteRenderer();
            if (_spriteRenderer == null || _teamIndicatorRenderer == null)
            {
                return;
            }

            _teamIndicatorRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
            _teamIndicatorRenderer.sortingOrder = _spriteRenderer.sortingOrder + 10;
        }

        private static bool TryGetIndicatorColor(string targetMaster, out Color indicatorColor)
        {
            switch (targetMaster)
            {
                case LeftPlayer:
                    indicatorColor = LeftIndicatorColor;
                    return true;
                case RightPlayer:
                    indicatorColor = RightIndicatorColor;
                    return true;
                default:
                    indicatorColor = Color.clear;
                    return false;
            }
        }

        private static Sprite GetTeamIndicatorSprite()
        {
            if (_teamIndicatorSprite != null)
            {
                return _teamIndicatorSprite;
            }

            _teamIndicatorSprite = Resources.Load<Sprite>(TeamIndicatorResourcePath);
            if (_teamIndicatorSprite == null)
            {
                WDebug.LogWarning($"Team indicator sprite not found at Resources/{TeamIndicatorResourcePath}.");
            }

            return _teamIndicatorSprite;
        }

        private void DestroySelf()
        {
            OnDestroyed?.Invoke();
            Destroy(gameObject);
        }
        
        private void DestroySelf(float delay)
        {
            StartCoroutine(DelayedDestroySelfCoroutine(delay));
        }
        
        private IEnumerator DelayedDestroySelfCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            DestroySelf();
        }
    }
}
