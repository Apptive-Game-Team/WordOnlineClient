using UnityEngine;

namespace Script.GameScene
{
    public static class DamagedObjectEffect
    {
        public static void SetSelfDestroyEffect(string effect, Transform tr)
        {
            GameObject effectPrefab = (GameObject) Resources.Load($"Prefabs/Effects/{effect}");
            
            if (effectPrefab == null)
            {
                Debug.LogWarning($"Effect prefab '{effect}' not found.");
                return;
            }
            
            GameObject.Instantiate(effectPrefab, tr.position, Quaternion.identity);
        }
    }
}