using DG.Tweening;
using UnityEngine;

namespace Script.GameScene
{
    public class ServedObject : MonoBehaviour
    {
        private const float BounceScale = 1.3f;
        private const float SquashScale = 0.8f;
        private const float Duration = 0.2f;

        private Vector3 originalScale;
        public int id;
        private GameObject _effectInstance = null;
        public int hp;
        public int maxHp;
        private string master;

        private int lastHp = -1;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        public void SetMaster(string master)
        {
            this.master = master;
            if (!SceneContext.Me.Equals(master) && master != "None")
            {
                gameObject.GetComponentInChildren<SpriteRenderer>().color = new Color(1f, 0.5f, 0.5f, 1f);
            }
            
            if (master.Equals("RightPlayer"))
            {
                if (transform.rotation.eulerAngles.y == 0)
                {
                    gameObject.GetComponentInChildren<SpriteRenderer>().flipX = true;
                    return;
                }
                gameObject.transform.Rotate(0, 180, 0);
            }
        }

        public void UpdateObject(UpdatedObjectDto updatedObjectDto)
        {
            transform.position = new Vector3(
                updatedObjectDto.position.x, 
                updatedObjectDto.position.y, 
                updatedObjectDto.position.z);
            hp = updatedObjectDto.hp;
            maxHp = updatedObjectDto.maxHp;
            if (updatedObjectDto.status.Equals("Destroyed"))
            {
                ObjectContainer.Instance.UnregisterObject(this);
                Destroy(gameObject);
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
        
        private void SetEffect(string effect)
        {
            if (effect.Equals("None"))
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
                Debug.LogWarning($"Effect prefab '{effect}' not found.");
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
                SetSelfDestroyEffect("HitEffect");
                PlayBounce();
            }
            lastHp = hp;
        }
        
        private void PlayBounce()
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOScale(new Vector3(originalScale.x * SquashScale, originalScale.y * BounceScale, originalScale.z), Duration / 2)
                .SetEase(Ease.OutQuad));
            seq.Append(transform.DOScale(originalScale, Duration / 2).SetEase(Ease.InQuad));
        }
        
        private void SetSelfDestroyEffect(string effect)
        {
            GameObject effectPrefab = (GameObject) Resources.Load($"Prefabs/Effects/{effect}");
            
            if (effectPrefab == null)
            {
                Debug.LogWarning($"Effect prefab '{effect}' not found.");
                return;
            }
            
            Instantiate(effectPrefab, transform.position, Quaternion.identity);
        }
    }
}