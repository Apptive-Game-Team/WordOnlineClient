#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using GameScene.ServedObjectComponent.Sound;
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
        var prefabs = LoadRuntimePrefabs();
        foreach (KeyValuePair<string, GameObject> prefab in prefabs)
        {
            RuntimeSfxArchetype archetype = ObjectSfxRuntimeTypeCatalog.Resolve(prefab.Key);
            if (archetype == RuntimeSfxArchetype.Silent &&
                prefab.Key != "ServedObjectHpBar" && prefab.Key != "Towerback")
            {
                errors.Add($"Top-level prefab is missing an explicit audible mapping: {prefab.Key}.");
            }
        }

        if (ObjectSfxRuntimeTypeCatalog.Resolve("__UnknownRuntimeType__") != RuntimeSfxArchetype.Silent)
        {
            errors.Add("Unknown runtime types must resolve to silence.");
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

}
#endif
