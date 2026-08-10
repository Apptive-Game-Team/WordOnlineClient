using Global;
using UnityEngine;

namespace GameScene.ServedObjectComponent
{
    public static class DamagedObjectEffect
    {
        public static void SetSelfDestroyEffect(string effect, Transform tr)
        {
            SetSelfDestroyEffect(effect, tr.position);
        }

        public static void SetSelfDestroyEffect(string effect, Vector3 worldPosition)
        {
            GameObject effectPrefab = (GameObject) Resources.Load($"Prefabs/Effects/{effect}");

            if (effectPrefab == null)
            {
                WDebug.LogWarning($"Effect prefab '{effect}' not found.");
                return;
            }

            UnityEngine.Object.Instantiate(effectPrefab, worldPosition, Quaternion.identity);
        }
    }
}
