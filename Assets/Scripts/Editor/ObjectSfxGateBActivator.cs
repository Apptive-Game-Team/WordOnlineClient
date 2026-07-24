#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Sound.Config;
using UnityEditor;
using UnityEngine;

public static class ObjectSfxGateBActivator
{
    private const string ProfileRoot = "Assets/Resources/Sound/Config/Profiles";

    [MenuItem("Tools/Sound/Apply Gate B Profile Clips")]
    public static void Apply()
    {
        ApplyProfile("FireCreature", new Dictionary<string, string>
        {
            { "spawn", "Assets/Resources/Sound/Game/Fire/fire_spawn_v1.wav" },
            { "attack", "Assets/Resources/Sound/Game/Fire/fire_attack_v1.wav" },
            { "hit", "Assets/Resources/Sound/Game/Fire/fire_hit_v1.wav" },
            { "death", "Assets/Resources/Sound/Game/Fire/fire_death_v1.wav" }
        });
        ApplyProfile("StoneBuilding", new Dictionary<string, string>
        {
            { "spawn", "Assets/Resources/Sound/Game/Stone/stone_spawn_v1.wav" },
            { "hit", "Assets/Resources/Sound/Game/Stone/stone_hit_v1.wav" },
            { "death", "Assets/Resources/Sound/Game/Stone/stone_death_v1.wav" }
        });
        AssetDatabase.SaveAssets();

        List<string> errors = ObjectSfxCatalogValidator.Validate();
        if (errors.Count > 0)
        {
            foreach (string error in errors)
            {
                Debug.LogError(error);
            }
            throw new InvalidOperationException(
                $"Gate B activation failed validation with {errors.Count} error(s).");
        }

        Debug.Log("Gate B profile clips applied and validated.");
    }

    private static void ApplyProfile(string profileId, Dictionary<string, string> slotClips)
    {
        string path = $"{ProfileRoot}/{profileId}.asset";
        ObjectSfxProfile profile = AssetDatabase.LoadAssetAtPath<ObjectSfxProfile>(path);
        if (profile == null)
        {
            throw new InvalidOperationException($"Profile asset missing: {path}");
        }

        SerializedObject serialized = new(profile);
        foreach (KeyValuePair<string, string> pair in slotClips)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(pair.Value);
            if (clip == null)
            {
                throw new InvalidOperationException($"Clip missing: {pair.Value}");
            }

            SerializedProperty slot = serialized.FindProperty(pair.Key);
            slot.FindPropertyRelative("enabled").boolValue = true;
            slot.FindPropertyRelative("clip").objectReferenceValue = clip;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(profile);
    }
}
#endif
