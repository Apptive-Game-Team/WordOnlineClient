#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using GameScene.ServedObjectComponent.OnAttack;
using Sound.Config;
using UnityEditor;
using UnityEngine;

public static class ObjectSfxCatalogValidator
{
    private const string PrefabRoot = "Assets/Resources/Prefabs";

    [MenuItem("Tools/Sound/Validate Object SFX Catalog")]
    public static void ValidateFromMenu()
    {
        List<string> errors = Validate();
        if (errors.Count == 0)
        {
            Debug.Log("Object SFX catalog validation passed.");
            return;
        }

        foreach (string error in errors)
        {
            Debug.LogError(error);
        }

        throw new InvalidOperationException(
            $"Object SFX catalog validation failed with {errors.Count} error(s).");
    }

    public static List<string> Validate()
    {
        var errors = new List<string>();
        ObjectSfxCatalog catalog = Resources.Load<ObjectSfxCatalog>(ObjectSfxCatalog.ResourcesPath);
        if (catalog == null)
        {
            errors.Add($"Missing catalog at Resources/{ObjectSfxCatalog.ResourcesPath}.");
            return errors;
        }

        var prefabs = LoadRuntimePrefabs();
        var entries = new Dictionary<string, ObjectSfxCatalogEntry>(StringComparer.Ordinal);

        foreach (ObjectSfxCatalogEntry entry in catalog.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.RuntimeType))
            {
                errors.Add("Catalog contains an empty runtime type.");
                continue;
            }

            if (!entries.TryAdd(entry.RuntimeType, entry))
            {
                errors.Add($"Duplicate runtime type: {entry.RuntimeType}.");
                continue;
            }

            if (!entry.IntentionalSilent && entry.Profile == null)
            {
                errors.Add($"{entry.RuntimeType} has neither a profile nor intentional silence.");
            }

            if (entry.IntentionalSilent && entry.Profile != null)
            {
                errors.Add($"{entry.RuntimeType} is silent but also has a profile.");
            }

            if (!prefabs.ContainsKey(entry.RuntimeType) && !entry.ServerAlias)
            {
                errors.Add($"Catalog runtime type has no top-level prefab: {entry.RuntimeType}.");
            }

            ValidateProfile(entry, prefabs, errors);
        }

        foreach (string runtimeType in prefabs.Keys)
        {
            if (!entries.ContainsKey(runtimeType))
            {
                errors.Add($"Top-level prefab has no catalog row: {runtimeType}.");
            }
        }

        return errors;
    }

    private static Dictionary<string, GameObject> LoadRuntimePrefabs()
    {
        var prefabs = new Dictionary<string, GameObject>(StringComparer.Ordinal);
        foreach (string assetPath in Directory.GetFiles(PrefabRoot, "*.prefab", SearchOption.TopDirectoryOnly))
        {
            string normalizedPath = assetPath.Replace('\\', '/');
            string runtimeType = Path.GetFileNameWithoutExtension(normalizedPath);
            prefabs[runtimeType] = AssetDatabase.LoadAssetAtPath<GameObject>(normalizedPath);
        }

        return prefabs;
    }

    private static void ValidateProfile(
        ObjectSfxCatalogEntry entry,
        IReadOnlyDictionary<string, GameObject> prefabs,
        ICollection<string> errors)
    {
        ObjectSfxProfile profile = entry.Profile;
        if (profile == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(profile.ProfileId))
        {
            errors.Add($"{entry.RuntimeType} uses a profile without an ID.");
        }

        ValidateSlot(entry.RuntimeType, "spawn", profile.Spawn, errors);
        ValidateSlot(entry.RuntimeType, "movement", profile.Movement, errors);
        ValidateSlot(entry.RuntimeType, "attack", profile.Attack, errors);
        ValidateSlot(entry.RuntimeType, "hit", profile.Hit, errors);
        ValidateSlot(entry.RuntimeType, "heal", profile.Heal, errors);
        ValidateSlot(entry.RuntimeType, "death", profile.Death, errors);

        if (profile.IsStatic && profile.Movement.Enabled)
        {
            errors.Add($"Static profile enables movement: {entry.RuntimeType}.");
        }

        if (profile.IsProjectileOrTransient && profile.Movement.Enabled)
        {
            errors.Add($"Projectile/transient profile enables lifecycle movement: {entry.RuntimeType}.");
        }

        if (profile.Attack.Enabled &&
            prefabs.TryGetValue(entry.RuntimeType, out GameObject prefab) &&
            prefab != null &&
            prefab.GetComponentInChildren<OnAttackSoundPlayer>(true) != null)
        {
            errors.Add($"Duplicate attack ownership on {entry.RuntimeType}.");
        }
    }

    private static void ValidateSlot(
        string runtimeType,
        string eventName,
        ObjectSfxEventSlot slot,
        ICollection<string> errors)
    {
        if (slot.Enabled && slot.Clip == null)
        {
            errors.Add($"{runtimeType} enables {eventName} without a clip.");
        }
    }
}
#endif
