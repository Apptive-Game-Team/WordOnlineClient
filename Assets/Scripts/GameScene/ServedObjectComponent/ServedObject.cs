using System;
using System.Collections;
using Data;
using GameScene.Object;
using Global;
using UnityEngine;

namespace GameScene.ServedObjectComponent
{
    public class ServedObject : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Transform _actualTransform = null;
        public int id;
        private GameObject _effectInstance = null;
        public int hp;
        public int maxHp;
        private string master;

        private PositionUpdater _positionUpdater;

        public event Action OnAttack;
        public event Action OnDamaged;
        public event Action<string> OnOtherStatus;
        public event Action OnDestroyed;
        public event Action<int> OnHpChanged;
        
        public event Action OnHpIncreased;
        public event Action OnHpDecreased;

        private int lastHp = 0;
        
        public void SetMaster(string master)
        {
            this.master = master;
            
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            
            if (_spriteRenderer == null)
            {
                return;
            }
            
            _positionUpdater = new PositionUpdater(transform, _spriteRenderer);
            
            if (!SceneContext.Me.Equals(master) && master != "None")
            {
                _spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 1f);
            }
            
            if (master.Equals("RightPlayer"))
            {
                if (transform.rotation.eulerAngles.y == 0)
                {
                    _spriteRenderer.flipX = true;
                    return;
                }
                gameObject.transform.Rotate(0, 180, 0);
            }
        }
        
        public string GetMaster()
        {
            return master;
        }

        public void UpdateObject(UpdatedObjectDto updatedObjectDto)
        {
            _positionUpdater?.UpdatePosition(updatedObjectDto);

            if (hp != updatedObjectDto.hp)
            {
                OnHpChanged?.Invoke(updatedObjectDto.hp);
            }
            hp = updatedObjectDto.hp;
            maxHp = updatedObjectDto.maxHp;

            HandleStatus(updatedObjectDto.status);
            
            // TODO - Add Logic for Animation, State, Effect Atc
            SetEffect(updatedObjectDto.effect);
            HandleDamageEffect();
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
        
        private void SetEffect(string effect)
        {
            if (effect.Equals("None") || string.IsNullOrEmpty(effect))
            {
                if (_effectInstance != null)
                {
                    Destroy(_effectInstance);
                    _effectInstance = null;
                }
                return;
            }
            
            GameObject effectPrefab = (GameObject) Resources.Load($"Prefabs/Effects/{effect}");
            if (effectPrefab == null)
            {
                WDebug.LogWarning($"Effect prefab '{effect}' not found.");
                return;
            }
            if (_effectInstance != null)
            {
                Destroy(_effectInstance);
            }
            
            Transform actualTransform = GetActualTransform();
            _effectInstance = Instantiate(effectPrefab, actualTransform.position, Quaternion.identity);
            _effectInstance.transform.SetParent(actualTransform);
        }
        
        private Transform GetActualTransform()
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
            
            var simpleZVisualizer = GetComponentInChildren<SimpleZVisualizer>();
            if (simpleZVisualizer != null)
            {
                _actualTransform = simpleZVisualizer.ActualTransform;
                return _actualTransform;
            }
            _actualTransform = transform;
            return _actualTransform;
        }

        public Vector3 GetSpeechBubbleAnchorWorldPosition(float verticalOffset = 0.15f)
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (_spriteRenderer != null)
            {
                Bounds bounds = _spriteRenderer.bounds;
                return new Vector3(bounds.center.x, bounds.max.y + verticalOffset, GetActualTransform().position.z);
            }

            return GetActualTransform().position + Vector3.up * (1f + verticalOffset);
        }
        
        private void HandleDamageEffect()
        {
            if (hp < lastHp)
            {
                OnHpDecreased?.Invoke();
            }
            if (hp > lastHp && hp != maxHp)
            {
                OnHpIncreased?.Invoke();
            }
            lastHp = hp;
        }

        private void DestroySelf()
        {
            OnDestroyed?.Invoke();
            ObjectContainer.Instance.UnregisterObject(id);
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
