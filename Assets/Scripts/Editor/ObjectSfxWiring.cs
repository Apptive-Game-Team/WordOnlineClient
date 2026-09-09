#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Sound.Config;
using UnityEditor;
using UnityEngine;

public static class ObjectSfxWiring
{
    private const string ApprovedSfxJsonPath = "Assets/Resources/Sound/Config/approved-sfx.json";
    private const string ProfileRoot = "Assets/Resources/Sound/Config/Profiles";
    private const string SignatureProfileRoot = ProfileRoot + "/Signatures";
    private const string CatalogPath = "Assets/Resources/Sound/Config/ObjectSfxCatalog.asset";

    private const string TargetTypeProfile = "profile";
    private const string TargetTypeSignature = "signature";
    private const string TargetTypeBuildingDeath = "buildingDeath";

    private static readonly HashSet<string> ValidSlotNames = new(StringComparer.Ordinal)
    {
        "spawn", "movement", "attack", "hit", "heal", "death"
    };

    [MenuItem("Tools/Sound/Wire Approved SFX")]
    public static void WireApprovedSfx()
    {
        ApprovedSfxFile approved = LoadApprovedSfx();
        ObjectSfxCatalog catalog = LoadCatalog();
        SerializedObject serializedCatalog = new(catalog);

        var errors = new List<string>();
        int wiredCount = 0;
        int skippedCount = 0;

        foreach (ApprovedSfxEntry entry in approved.Entries)
        {
            if (WireEntry(entry, serializedCatalog, errors))
            {
                wiredCount++;
            }
            else
            {
                skippedCount++;
            }
        }

        serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Wired {wiredCount} approved SFX entries, skipped {skippedCount}.");

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Wiring approved SFX found {errors.Count} error(s):\n" + string.Join("\n", errors));
        }
    }

    private static bool WireEntry(
        ApprovedSfxEntry entry,
        SerializedObject serializedCatalog,
        List<string> errors)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(entry.ClipPath);
        if (clip == null)
        {
            errors.Add($"Clip not found, skipping '{entry.Event}': {entry.ClipPath}.");
            return false;
        }

        string targetType = entry.Target?.Type;
        return targetType switch
        {
            TargetTypeProfile => WireProfileTarget(entry, clip, errors),
            TargetTypeSignature => WireSignatureTarget(entry, clip, serializedCatalog, errors),
            TargetTypeBuildingDeath => WireBuildingDeathTarget(entry, clip, errors),
            _ => FailUnknownTargetType(entry, targetType, errors)
        };
    }

    private static bool FailUnknownTargetType(ApprovedSfxEntry entry, string targetType, List<string> errors)
    {
        errors.Add($"Unknown target type '{targetType}' for '{entry.Event}'.");
        return false;
    }

    private static bool WireProfileTarget(ApprovedSfxEntry entry, AudioClip clip, List<string> errors)
    {
        string profilePath = $"{ProfileRoot}/{entry.Target.Profile}.asset";
        ObjectSfxProfile profile = AssetDatabase.LoadAssetAtPath<ObjectSfxProfile>(profilePath);
        if (profile == null)
        {
            errors.Add($"Profile target not found for '{entry.Event}': {profilePath}.");
            return false;
        }

        return WireSlot(profile, entry.Target.Slot, clip, entry.Volume, entry.Event, errors);
    }

    // The owning profile's faction/element/archetype are copied onto the signature so the
    // validator's static/transient rules see a consistent identity, not a blank one.
    private static bool WireSignatureTarget(
        ApprovedSfxEntry entry,
        AudioClip clip,
        SerializedObject serializedCatalog,
        List<string> errors)
    {
        string runtimeType = entry.Target.RuntimeType;
        SerializedProperty catalogEntry = FindCatalogEntry(serializedCatalog, runtimeType);
        if (catalogEntry == null)
        {
            errors.Add($"Catalog has no row for runtime type '{runtimeType}' (event '{entry.Event}').");
            return false;
        }

        var ownerProfile =
            catalogEntry.FindPropertyRelative("profile").objectReferenceValue as ObjectSfxProfile;
        if (ownerProfile == null)
        {
            errors.Add(
                $"Runtime type '{runtimeType}' has no base profile to copy faction/element/archetype " +
                $"from (event '{entry.Event}'); cannot create its signature profile.");
            return false;
        }

        ObjectSfxProfile signatureProfile = GetOrCreateSignatureProfile(runtimeType, ownerProfile);
        if (!WireSlot(signatureProfile, entry.Target.Slot, clip, entry.Volume, entry.Event, errors))
        {
            return false;
        }

        catalogEntry.FindPropertyRelative("signature").objectReferenceValue = signatureProfile;
        return true;
    }

    // Building death is a shared sound: every Building-archetype profile gets the same clip.
    private static bool WireBuildingDeathTarget(ApprovedSfxEntry entry, AudioClip clip, List<string> errors)
    {
        List<ObjectSfxProfile> buildingProfiles = FindBuildingProfiles();
        if (buildingProfiles.Count == 0)
        {
            errors.Add($"No Building-archetype profiles found for shared building death (event '{entry.Event}').");
            return false;
        }

        bool allWired = true;
        foreach (ObjectSfxProfile profile in buildingProfiles)
        {
            if (!WireSlot(profile, "death", clip, entry.Volume, entry.Event, errors))
            {
                allWired = false;
            }
        }

        return allWired;
    }

    private static bool WireSlot(
        ObjectSfxProfile profile,
        string slotName,
        AudioClip clip,
        float volume,
        string eventName,
        List<string> errors)
    {
        if (!ValidSlotNames.Contains(slotName))
        {
            errors.Add($"Unknown slot '{slotName}' for '{eventName}' on profile '{profile.ProfileId}'.");
            return false;
        }

        SerializedObject serializedProfile = new(profile);
        SerializedProperty slot = serializedProfile.FindProperty(slotName);
        slot.FindPropertyRelative("enabled").boolValue = true;
        slot.FindPropertyRelative("clip").objectReferenceValue = clip;
        slot.FindPropertyRelative("volume").floatValue = volume;
        serializedProfile.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(profile);
        return true;
    }

    private static ObjectSfxProfile GetOrCreateSignatureProfile(string runtimeType, ObjectSfxProfile ownerProfile)
    {
        EnsureFolder(SignatureProfileRoot);
        string path = $"{SignatureProfileRoot}/{runtimeType}.asset";
        ObjectSfxProfile profile = AssetDatabase.LoadAssetAtPath<ObjectSfxProfile>(path);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<ObjectSfxProfile>();
            AssetDatabase.CreateAsset(profile, path);
        }

        SerializedObject serializedProfile = new(profile);
        serializedProfile.FindProperty("profileId").stringValue = $"Signature_{runtimeType}";
        serializedProfile.FindProperty("faction").enumValueIndex = (int)ownerProfile.Faction;
        serializedProfile.FindProperty("element").enumValueIndex = (int)ownerProfile.Element;
        serializedProfile.FindProperty("archetype").enumValueIndex = (int)ownerProfile.Archetype;
        serializedProfile.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(profile);
        return profile;
    }

    // Direct children only: a recursive search would also match Profiles/Signatures, and a
    // signature profile is never a shared building-death target.
    private static List<ObjectSfxProfile> FindBuildingProfiles()
    {
        var profiles = new List<ObjectSfxProfile>();
        foreach (string guid in AssetDatabase.FindAssets($"t:{nameof(ObjectSfxProfile)}", new[] { ProfileRoot }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetDirectoryName(path)?.Replace('\\', '/') != ProfileRoot)
            {
                continue;
            }

            ObjectSfxProfile profile = AssetDatabase.LoadAssetAtPath<ObjectSfxProfile>(path);
            if (profile != null && profile.Archetype == ObjectSfxArchetype.Building)
            {
                profiles.Add(profile);
            }
        }

        return profiles;
    }

    private static SerializedProperty FindCatalogEntry(SerializedObject serializedCatalog, string runtimeType)
    {
        SerializedProperty entries = serializedCatalog.FindProperty("entries");
        for (int index = 0; index < entries.arraySize; index++)
        {
            SerializedProperty entry = entries.GetArrayElementAtIndex(index);
            string entryRuntimeType = entry.FindPropertyRelative("runtimeType").stringValue;
            if (string.Equals(entryRuntimeType, runtimeType, StringComparison.Ordinal))
            {
                return entry;
            }
        }

        return null;
    }

    private static ApprovedSfxFile LoadApprovedSfx()
    {
        TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(ApprovedSfxJsonPath);
        if (json == null)
        {
            throw new InvalidOperationException($"Approved SFX JSON not found: {ApprovedSfxJsonPath}.");
        }

        ApprovedSfxFile approved = JsonConvert.DeserializeObject<ApprovedSfxFile>(json.text);
        if (approved?.Entries == null)
        {
            throw new InvalidOperationException($"Approved SFX JSON has no entries: {ApprovedSfxJsonPath}.");
        }

        return approved;
    }

    private static ObjectSfxCatalog LoadCatalog()
    {
        ObjectSfxCatalog catalog = AssetDatabase.LoadAssetAtPath<ObjectSfxCatalog>(CatalogPath);
        if (catalog == null)
        {
            throw new InvalidOperationException(
                "Object SFX catalog is missing; run 'Tools/Sound/Create or Update Baseline Object SFX " +
                $"Catalog' first: {CatalogPath}.");
        }

        return catalog;
    }

    private static void EnsureFolder(string folderPath)
    {
        string[] segments = folderPath.Split('/');
        string currentPath = segments[0];
        for (int index = 1; index < segments.Length; index++)
        {
            string nextPath = $"{currentPath}/{segments[index]}";
            if (!AssetDatabase.IsValidFolder(nextPath))
            {
                AssetDatabase.CreateFolder(currentPath, segments[index]);
            }
            currentPath = nextPath;
        }
    }

    [Serializable]
    private sealed class ApprovedSfxFile
    {
        [JsonProperty("entries")] public List<ApprovedSfxEntry> Entries;
    }

    [Serializable]
    private sealed class ApprovedSfxEntry
    {
        [JsonProperty("event")] public string Event;
        [JsonProperty("clipPath")] public string ClipPath;
        [JsonProperty("volume")] public float Volume;
        [JsonProperty("target")] public SfxTarget Target;
    }

    [Serializable]
    private sealed class SfxTarget
    {
        [JsonProperty("type")] public string Type;
        [JsonProperty("profile")] public string Profile;
        [JsonProperty("runtimeType")] public string RuntimeType;
        [JsonProperty("slot")] public string Slot;
    }
}
#endif
