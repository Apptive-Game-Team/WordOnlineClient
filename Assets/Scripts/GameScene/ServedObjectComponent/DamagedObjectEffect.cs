using Global;
using UnityEngine;

namespace GameScene.ServedObjectComponent
{
    public static class DamagedObjectEffect
    {
        public static void SetSelfDestroyEffect(string effect, Transform tr, Vector3 offset = default)
        {
            GameObject effectPrefab = (GameObject) Resources.Load($"Prefabs/Effects/{effect}");
            
            if (effectPrefab == null)
            {
                WDebug.LogWarning($"Effect prefab '{effect}' not found.");
                return;
            }
            
            UnityEngine.Object.Instantiate(effectPrefab, tr.position + offset, Quaternion.identity);
        }
    }
}