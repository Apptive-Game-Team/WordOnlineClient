using UnityEngine;

namespace Script.GameScene
{
    public class ServedObject : MonoBehaviour
    {
        public int id;
        private GameObject _effectInstance = null;
        public int hp;
        public int maxHp;
        private string master;
        
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
            else
            // TODO - Add Logic for Animation, State, Effect Atc
            SetEffect(updatedObjectDto.effect);
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
    }
}