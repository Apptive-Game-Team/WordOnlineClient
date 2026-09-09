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
    private const string CatalogPath =
        "Assets/Resources/Sound/Config/ObjectSfxCatalog.asset";

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
        Dictionary<string, GameObject> prefabs = LoadRuntimePrefabs();
        ObjectSfxCatalog catalog = AssetDatabase.LoadAssetAtPath<ObjectSfxCatalog>(CatalogPath);
        if (catalog == null)
        {
            errors.Add($"Object SFX catalog is missing: {CatalogPath}.");
            return errors;
        }

        var mappedRuntimeTypes = new HashSet<string>(StringComparer.Ordinal);
        foreach (ObjectSfxCatalogEntry entry in catalog.Entries)
        {
            ValidateEntry(entry, prefabs, mappedRuntimeTypes, errors);
        }

        foreach (string runtimeType in prefabs.Keys)
        {
            if (!mappedRuntimeTypes.Contains(runtimeType))
            {
                errors.Add($"Top-level prefab has no explicit catalog row: {runtimeType}.");
            }
        }

        return errors;
    }

    private static void ValidateEntry(
        ObjectSfxCatalogEntry entry,
        IReadOnlyDictionary<string, GameObject> prefabs,
        ISet<string> mappedRuntimeTypes,
        ICollection<string> errors)
    {
        if (entry == null || string.IsNullOrWhiteSpace(entry.RuntimeType))
        {
            errors.Add("Catalog contains an entry with an empty runtime type.");
            return;
        }

        if (!mappedRuntimeTypes.Add(entry.RuntimeType))
        {
            errors.Add($"Catalog contains a duplicate runtime type: {entry.RuntimeType}.");
            return;
        }

        bool hasPrefab = prefabs.TryGetValue(entry.RuntimeType, out GameObject prefab);
        if (!hasPrefab && !entry.ServerAlias)
        {
            errors.Add($"Catalog row has no top-level prefab and is not a server alias: {entry.RuntimeType}.");
        }

        if (entry.Signature != null)
        {
            ValidateSignature(entry.Signature, entry.RuntimeType, errors);
        }

        if (entry.IntentionalSilent)
        {
            if (entry.Profile != null)
            {
                errors.Add($"Intentional-silence row must not reference a profile: {entry.RuntimeType}.");
            }
            return;
        }

        if (entry.Profile == null)
        {
            errors.Add($"Audible catalog row has no profile: {entry.RuntimeType}.");
            return;
        }

        ValidateProfile(entry.Profile, entry.RuntimeType, errors);
        if (hasPrefab)
        {
            ValidateEffectiveAttackOwner(prefab, entry.Profile, entry.RuntimeType, errors);
        }
    }

    private static void ValidateProfile(
        ObjectSfxProfile profile,
        string runtimeType,
        ICollection<string> errors)
    {
        ValidateSlot(profile.Spawn, "spawn", profile.ProfileId, runtimeType, errors);
        ValidateSlot(profile.Movement, "movement", profile.ProfileId, runtimeType, errors);
        ValidateSlot(profile.Attack, "attack", profile.ProfileId, runtimeType, errors);
        ValidateSlot(profile.Hit, "hit", profile.ProfileId, runtimeType, errors);
        ValidateSlot(profile.Heal, "heal", profile.ProfileId, runtimeType, errors);
        ValidateSlot(profile.Death, "death", profile.ProfileId, runtimeType, errors);

        if (profile.IsStatic && profile.Movement.Enabled)
        {
            errors.Add(
                $"Static profile '{profile.ProfileId}' enables movement for {runtimeType}.");
        }

        if (profile.IsProjectileOrTransient &&
            (profile.Movement.Enabled || profile.Hit.Enabled ||
             profile.Heal.Enabled || profile.Death.Enabled))
        {
            errors.Add(
                $"Projectile/transient profile '{profile.ProfileId}' enables lifecycle events for {runtimeType}.");
        }
    }

    // A signature replaces a base slot rather than layering on top of it, so a profile and its
    // signature enabling the same event is expected, not a conflict.
    private static void ValidateSignature(
        ObjectSfxProfile signature,
        string runtimeType,
        ICollection<string> errors)
    {
        ValidateSlot(signature.Spawn, "spawn", signature.ProfileId, runtimeType, errors);
        ValidateSlot(signature.Movement, "movement", signature.ProfileId, runtimeType, errors);
        ValidateSlot(signature.Attack, "attack", signature.ProfileId, runtimeType, errors);
        ValidateSlot(signature.Hit, "hit", signature.ProfileId, runtimeType, errors);
        ValidateSlot(signature.Heal, "heal", signature.ProfileId, runtimeType, errors);
        ValidateSlot(signature.Death, "death", signature.ProfileId, runtimeType, errors);

        // Attack, hit and movement fire many times a second; a signature there would be noise,
        // not identity, so the shared base owns those slots exclusively.
        if (signature.Attack.Enabled || signature.Hit.Enabled || signature.Movement.Enabled)
        {
            errors.Add(
                $"Signature '{signature.ProfileId}' enables a shared high-frequency slot " +
                $"(attack, hit or movement) for {runtimeType}.");
        }

        bool anySlotEnabled =
            signature.Spawn.Enabled || signature.Movement.Enabled || signature.Attack.Enabled ||
            signature.Hit.Enabled || signature.Heal.Enabled || signature.Death.Enabled;
        if (!anySlotEnabled)
        {
            errors.Add($"Signature '{signature.ProfileId}' has no slot enabled for {runtimeType}.");
        }
    }

    private static void ValidateSlot(
        ObjectSfxEventSlot slot,
        string eventName,
        string profileId,
        string runtimeType,
        ICollection<string> errors)
    {
        if (slot.Enabled && slot.Clip == null)
        {
            errors.Add(
                $"Profile '{profileId}' enables {eventName} without a clip for {runtimeType}.");
        }
    }

    private static void ValidateEffectiveAttackOwner(
        GameObject prefab,
        ObjectSfxProfile profile,
        string runtimeType,
        ICollection<string> errors)
    {
        bool profileOwnsAttack = profile.Attack.Enabled && profile.Attack.Clip != null;
        int legacyOwnerCount =
            prefab.GetComponentsInChildren<OnAttackSoundPlayer>(true).Length;
        int effectiveOwnerCount = profileOwnsAttack ? 1 : legacyOwnerCount;
        if (effectiveOwnerCount > 1)
        {
            errors.Add(
                $"Runtime type '{runtimeType}' has {effectiveOwnerCount} effective attack owners.");
        }
    }

    private static Dictionary<string, GameObject> LoadRuntimePrefabs()
    {
        var prefabs = new Dictionary<string, GameObject>(StringComparer.Ordinal);
        foreach (string assetPath in
                 Directory.GetFiles(PrefabRoot, "*.prefab", SearchOption.TopDirectoryOnly))
        {
            string normalizedPath = assetPath.Replace('\\', '/');
            string runtimeType = Path.GetFileNameWithoutExtension(normalizedPath);
            prefabs[runtimeType] =
                AssetDatabase.LoadAssetAtPath<GameObject>(normalizedPath);
        }

        return prefabs;
    }
}
#endif
