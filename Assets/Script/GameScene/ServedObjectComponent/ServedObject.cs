using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Script.GameScene.Object;
using Script.Global;
using UnityEngine;

namespace Script.GameScene
{
    public class ServedObject : MonoBehaviour
    {
        private const float BounceScale = 1.3f;
        private const float SquashScale = 0.8f;
        private const float Duration = 0.2f;
        private const float FRAME_DURATION = 0.1f;

        private Vector3 originalScale;
        public int id;
        private GameObject _effectInstance = null;
        public int hp;
        public int maxHp;
        private string master;
        
        private Vector3? nextPosition = null;
        private TweenerCore<Vector3, Vector3, VectorOptions> moveTween;

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

        public void UpdateObject(UpdatedObjectDto updatedObjectDto)
        {
            UpdatePosition(updatedObjectDto);
            
            hp = updatedObjectDto.hp;
            maxHp = updatedObjectDto.maxHp;
            if (updatedObjectDto.status.Equals("Destroyed"))
            {
                DestroySelf();
                return;
            }
            else if (updatedObjectDto.status.Equals("Attack"))
            {
                //feedback_ATTACK
                DOTweenAction.SwingMobAttack(transform.GetChild(0));
            }
            else
            // TODO - Add Logic for Animation, State, Effect Atc
            SetEffect(updatedObjectDto.effect);
            HandleDamageEffect();
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
            _effectInstance = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            _effectInstance.transform.SetParent(transform);
        }
        
        private void HandleDamageEffect()
        {
            if (hp < lastHp)
            {
                DamagedObjectEffect.SetSelfDestroyEffect("HitEffect",transform);
                DOTweenAction.BounceMob(transform);
            }
            if (hp > lastHp)
            {
                DamagedObjectEffect.SetSelfDestroyEffect("HealEffect",transform);
                DOTweenAction.BounceMob(transform);
            }
            lastHp = hp;
        }

        public void DestroySelf()
        {
            ObjectContainer.Instance.UnregisterObject(id);
        }
    }
}