using System;
using System.Collections.Generic;
using Global;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.ServedObjectComponent
{
    public class ServedObjectEffectRenderer
    {
        private const string NoEffect = "None";
        private const string OverchargeEffect = "Overcharge";
        private const string ShockEffect = "Shock";
        private const float StackedEffectAlpha = 0.65f;

        private readonly Func<Transform> actualTransformProvider;
        private readonly Func<float> objectSizeProvider;
        private readonly float scaleReferenceHeight;
        private readonly float scaleMultiplier;
        private readonly float scaleMin;
        private readonly float scaleMax;
        private readonly List<GameObject> effectInstances = new List<GameObject>();
        private readonly List<string> activeEffects = new List<string>();

        public ServedObjectEffectRenderer(
            Func<Transform> actualTransformProvider,
            Func<float> objectSizeProvider,
            float scaleReferenceHeight,
            float scaleMultiplier,
            float scaleMin,
            float scaleMax)
        {
            this.actualTransformProvider = actualTransformProvider;
            this.objectSizeProvider = objectSizeProvider;
            this.scaleReferenceHeight = scaleReferenceHeight;
            this.scaleMultiplier = scaleMultiplier;
            this.scaleMin = scaleMin;
            this.scaleMax = scaleMax;
        }

        public void SetEffects(List<string> effects)
        {
            List<string> normalizedEffects = NormalizeEffects(effects);
            if (AreSameEffects(activeEffects, normalizedEffects))
            {
                return;
            }

            ClearEffects();
            activeEffects.AddRange(normalizedEffects);

            Transform actualTransform = actualTransformProvider?.Invoke();
            if (actualTransform == null)
            {
                return;
            }

            HashSet<string> spawnedResourceNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (string effect in activeEffects)
            {
                string resourceName = ResolveResourceName(effect);
                if (spawnedResourceNames.Add(resourceName))
                {
                    SpawnEffect(effect, resourceName, actualTransform);
                }
            }
        }

        private void SpawnEffect(string effect, string resourceName, Transform actualTransform)
        {
            GameObject effectPrefab = Resources.Load<GameObject>($"Prefabs/Effects/{resourceName}");
            if (effectPrefab == null)
            {
                WDebug.LogWarning($"Effect prefab '{resourceName}' for '{effect}' not found.");
                return;
            }

            GameObject effectInstance = UnityEngine.Object.Instantiate(effectPrefab, actualTransform);
            effectInstance.transform.localPosition = Vector3.zero;
            effectInstance.transform.localRotation = Quaternion.identity;
            ApplyEffectScale(effectInstance.transform);
            ApplyEffectAlpha(effectInstance);
            effectInstances.Add(effectInstance);
        }

        private static string ResolveResourceName(string effect)
        {
            return string.Equals(effect, OverchargeEffect, StringComparison.Ordinal)
                ? ShockEffect
                : effect;
        }

        private void ClearEffects()
        {
            foreach (GameObject effectInstance in effectInstances)
            {
                if (effectInstance != null)
                {
                    UnityEngine.Object.Destroy(effectInstance);
                }
            }

            effectInstances.Clear();
            activeEffects.Clear();
        }

        private static List<string> NormalizeEffects(List<string> effects)
        {
            List<string> normalizedEffects = new List<string>();
            if (effects == null)
            {
                return normalizedEffects;
            }

            foreach (string effect in effects)
            {
                if (string.IsNullOrWhiteSpace(effect))
                {
                    continue;
                }

                string normalizedEffect = effect.Trim();
                if (string.Equals(normalizedEffect, NoEffect, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!normalizedEffects.Contains(normalizedEffect))
                {
                    normalizedEffects.Add(normalizedEffect);
                }
            }

            return normalizedEffects;
        }

        private static bool AreSameEffects(List<string> currentEffects, List<string> nextEffects)
        {
            if (currentEffects.Count != nextEffects.Count)
            {
                return false;
            }

            for (int i = 0; i < currentEffects.Count; i++)
            {
                if (!string.Equals(currentEffects[i], nextEffects[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private void ApplyEffectScale(Transform effectTransform)
        {
            if (effectTransform == null)
            {
                return;
            }

            float objectSize = objectSizeProvider?.Invoke() ?? 0f;
            if (objectSize <= 0f || scaleReferenceHeight <= 0f)
            {
                effectTransform.localScale = Vector3.one;
                return;
            }

            float scaleFactor = objectSize / scaleReferenceHeight;
            scaleFactor *= scaleMultiplier;
            scaleFactor = Mathf.Clamp(scaleFactor, scaleMin, scaleMax);
            effectTransform.localScale = Vector3.one * scaleFactor;
        }

        private static void ApplyEffectAlpha(GameObject effectInstance)
        {
            foreach (SpriteRenderer spriteRenderer in effectInstance.GetComponentsInChildren<SpriteRenderer>(true))
            {
                Color color = spriteRenderer.color;
                color.a = StackedEffectAlpha;
                spriteRenderer.color = color;
            }

            foreach (Graphic graphic in effectInstance.GetComponentsInChildren<Graphic>(true))
            {
                Color color = graphic.color;
                color.a = StackedEffectAlpha;
                graphic.color = color;
            }
        }
    }
}
