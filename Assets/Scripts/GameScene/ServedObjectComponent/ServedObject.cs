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
        private ServedObjectHpBar _servedObjectHpBar;
        private readonly List<string> _simulationEffects = new List<string>();
        private readonly List<string> _localEffects = new List<string>();

        public event Action OnAttack;
        public event Action OnDamaged;
        public event Action<string> OnOtherStatus;
        public event Action OnDestroyed;
        public event Action<Gauge> OnGaugeChanged;
        
        public event Action OnHpIncreased;
        public event Action OnHpDecreased;

        private int lastHp = 0;
        
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
                    OnAttack?.Invoke();
                    DOTweenAction.SwingMobAttack(GetActualTransform());
                    break;

                case "Damaged":
                    OnDamaged?.Invoke();
                    break;

                default:
                    OnOtherStatus?.Invoke(status);
                    break;
            }
        }
        
        public Transform GetActualTransform()
        {
            if (_actualTransform != null)
            {
                return _actualTransform;
            }

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

            if (_spriteRenderer != null)
            {
                Bounds bounds = _spriteRenderer.bounds;
                return new Vector3(bounds.center.x, bounds.max.y + verticalOffset, GetActualTransform().position.z);
            }

            return GetActualTransform().position + Vector3.up * (1f + verticalOffset);
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
            if (TryUpdateHpBarTeamIndicator())
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

        private bool TryUpdateHpBarTeamIndicator()
        {
            if (_servedObjectHpBar == null)
            {
                _servedObjectHpBar = GetComponentInChildren<ServedObjectHpBar>();
            }

            if (_servedObjectHpBar == null)
            {
                return false;
            }

            _servedObjectHpBar.SetObjectIndicatorMaster(master);
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
