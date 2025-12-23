using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Script.GameScene.Object;
using Script.GameScene.ServedObjectComponent;
using Script.Global;
using UnityEngine;

namespace Script.GameScene
{
    public class ServedObject : MonoBehaviour
    {
        private const float BounceScale = 1.3f;
        private const float SquashScale = 0.8f;
        private const float Duration = 0.2f;
        private const float FRAME_DURATION = 0.05f;

        private Vector3 originalScale;
        private Transform _actualTransform = null;
        public int id;
        private GameObject _effectInstance = null;
        public int hp;
        public int maxHp;
        private string master;
        
        private Vector3? nextPosition = null;
        private TweenerCore<Vector3, Vector3, VectorOptions> moveTween;

        public event Action OnAttack;
        public event Action OnDamaged;
        public event Action<string> OnOtherStatus;
        public event Action OnDestroyed;
        public event Action<int> OnHpChanged;

        private int lastHp = 0;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        public void SetMaster(string master)
        {
            this.master = master;
            SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
            
            if (renderer == null)
            {
                return;
            }
            
            if (!SceneContext.Me.Equals(master) && master != "None")
            {
                renderer.color = new Color(1f, 0.5f, 0.5f, 1f);
            }
            
            if (master.Equals("RightPlayer"))
            {
                if (transform.rotation.eulerAngles.y == 0)
                {
                    renderer.flipX = true;
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
            UpdatePosition(updatedObjectDto);

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
                    DestroySelf(FRAME_DURATION);
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

        private void UpdatePosition(UpdatedObjectDto updatedObjectDto)
        {
            if (moveTween != null && moveTween.IsActive())
            {
                moveTween.Kill();
            }
            if (nextPosition.HasValue)
            {
                transform.position = nextPosition.Value;
            }
            
            nextPosition = new Vector3(
                updatedObjectDto.position.x, 
                updatedObjectDto.position.y, 
                updatedObjectDto.position.z);
            moveTween = transform.DOMove(nextPosition.Value, FRAME_DURATION).SetEase(Ease.Linear);
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
            
            var simpleZVisualizer = GetComponentInChildren<SimpleZVisualizer>();
            if (simpleZVisualizer != null)
            {
                _actualTransform = simpleZVisualizer.ActualTransform;
                return _actualTransform;
            }
            _actualTransform = transform;
            return _actualTransform;
        }
        
        private void HandleDamageEffect()
        {
            if (hp < lastHp)
            {
                DamagedObjectEffect.SetSelfDestroyEffect("HitEffect",transform);
                DOTweenAction.BounceMob(transform);
            }
            if (hp > lastHp && hp != maxHp)
            {
                DamagedObjectEffect.SetSelfDestroyEffect("HealEffect",transform);
                DOTweenAction.BounceMob(transform);
            }
            lastHp = hp;
        }

        public void DestroySelf()
        {
            OnDestroyed?.Invoke();
            ObjectContainer.Instance.UnregisterObject(id);
        }
        
        public void DestroySelf(float delay)
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