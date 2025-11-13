using Script.Global;
using UnityEngine;

namespace Script.GameScene.ServedObjectComponent
{
    public static class DamagedObjectEffect
    {
        public static void SetSelfDestroyEffect(string effect, Transform tr)
        {
            GameObject effectPrefab = (GameObject) Resources.Load($"Prefabs/Effects/{effect}");
            
            if (effectPrefab == null)
            {
                WDebug.LogWarning($"Effect prefab '{effect}' not found.");
                return;
            }
            
            UnityEngine.Object.Instantiate(effectPrefab, tr.position, Quaternion.identity);
        }
    }
}