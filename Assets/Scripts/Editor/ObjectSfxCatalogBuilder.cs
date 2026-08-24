#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Sound.Config;
using UnityEditor;
using UnityEngine;

public static class ObjectSfxCatalogBuilder
{
    private const string ConfigRoot = "Assets/Resources/Sound/Config";
    private const string ProfileRoot = ConfigRoot + "/Profiles";
    private const string CatalogPath = ConfigRoot + "/ObjectSfxCatalog.asset";

    // Profiles are cut by faction, not by element. `.art/WORLD.md` and the magic
    // concept pages define identity by where a unit came from, and the material
    // palette follows that: hellfire demons are cracking husks, not lit matches;
    // human structures are the only place metal is allowed. Element survives as
    // the sub-material that separates the three World Tree spirits.
    private static readonly ProfileDefinition[] ProfileDefinitions =
    {
        new("BurningLegionCreature", ObjectSfxFaction.BurningLegion,
            ObjectSfxElement.Fire, ObjectSfxArchetype.Creature),
        new("WorldTreeNatureCreature", ObjectSfxFaction.WorldTreeSpirit,
            ObjectSfxElement.Nature, ObjectSfxArchetype.Creature),
        new("WorldTreeLightningCreature", ObjectSfxFaction.WorldTreeSpirit,
            ObjectSfxElement.Lightning, ObjectSfxArchetype.Creature),
        new("WorldTreeWindCreature", ObjectSfxFaction.WorldTreeSpirit,
            ObjectSfxElement.Wind, ObjectSfxArchetype.Creature),
        new("WaterSlimeCreature", ObjectSfxFaction.WaterSlime,
            ObjectSfxElement.Water, ObjectSfxArchetype.Creature),
        new("RockGolemCreature", ObjectSfxFaction.RockGolemTribe,
            ObjectSfxElement.Rock, ObjectSfxArchetype.Creature),
        new("DimensionWandererCreature", ObjectSfxFaction.DimensionWanderer,
            ObjectSfxElement.Neutral, ObjectSfxArchetype.Creature),
        new("HumanInfantry", ObjectSfxFaction.HumanCivilization,
            ObjectSfxElement.Neutral, ObjectSfxArchetype.Creature),
        new("PlayerAvatar", ObjectSfxFaction.Neutral,
            ObjectSfxElement.Neutral, ObjectSfxArchetype.Creature),
        new("BurningLegionStructure", ObjectSfxFaction.BurningLegion,
            ObjectSfxElement.Fire, ObjectSfxArchetype.Building),
        new("WorldTreeBuilding", ObjectSfxFaction.WorldTreeSpirit,
            ObjectSfxElement.Nature, ObjectSfxArchetype.Building),
        new("WaterSlimeBuilding", ObjectSfxFaction.WaterSlime,
            ObjectSfxElement.Water, ObjectSfxArchetype.Building),
        new("RockGolemBuilding", ObjectSfxFaction.RockGolemTribe,
            ObjectSfxElement.Rock, ObjectSfxArchetype.Building),
        new("HumanStructure", ObjectSfxFaction.HumanCivilization,
            ObjectSfxElement.Neutral, ObjectSfxArchetype.Building),
        new("ArcaneRune", ObjectSfxFaction.Neutral,
            ObjectSfxElement.Neutral, ObjectSfxArchetype.Building),
        new("TransientLegacy", ObjectSfxFaction.Neutral,
            ObjectSfxElement.Neutral, ObjectSfxArchetype.TransientSpell),
        new("TransientShot", ObjectSfxFaction.Neutral,
            ObjectSfxElement.Neutral, ObjectSfxArchetype.TransientSpell),
        new("TransientExplode", ObjectSfxFaction.Neutral,
            ObjectSfxElement.Neutral, ObjectSfxArchetype.TransientSpell)
    };

    private static readonly CatalogGroup[] CatalogGroups =
    {
        // Demons of the hellfire dimension. FireTadpole is not one of them.
        new("BurningLegionCreature",
            "EmberSpirit", "FireChildSpirit", "FireLordSpirit", "FireSlime",
            "FireSpirit", "MagmaSpirit"),
        new("WorldTreeNatureCreature",
            "LeafSlime", "PveVineWitch", "SeedSpirit", "TreeGolem", "VineSpirit",
            "WillOWisp"),
        new("WorldTreeLightningCreature",
            "ElectricSlime", "StormRider", "ThunderBird", "ThunderSpirit",
            "ZapMouse"),
        new("WorldTreeWindCreature", "CloudDragon", "WindSlime", "WindSpirit"),
        new("WaterSlimeCreature", "AquaArcher", "BubbleSpirit", "WaterSlime"),
        new("RockGolemCreature", "RockGolem", "RockMage", "RockSlime"),
        // Three independent species that carry a piece of a collapsed world, not
        // a toad with its young: `.art/WORLD.md` says so in as many words.
        new("DimensionWandererCreature",
            "DimensionToad", "FireTadpole", "LightningTadpole"),
        new("HumanInfantry", "ChickenCommando"),
        // Heard more often than anything else, so it stays alone and plainest.
        new("PlayerAvatar", "Player"),
        new("BurningLegionStructure", "Crater", "FrenzyTotem"),
        new("WorldTreeBuilding",
            "ElectricTower", "GiantVine", "HealingTotem", "LifeTree", "ManaWell",
            "PveNatureSlimeNest", "PveVineColony", "SeedNest", "Vine", "VineColony",
            "WindTotem"),
        new("WaterSlimeBuilding", "BubbleGenerator", "PveWaterSlimeNest"),
        new("RockGolemBuilding", "RockTurret"),
        // The one faction whose art actually contains metal, so the one profile
        // allowed to sound like it. Non-ringing metal only.
        new("HumanStructure", "GroundCannon", "GroundTower", "RallyingTotem"),
        // Same shape on screen, element only. The faction spawn already says it.
        new("ArcaneRune",
            "FireRune", "LightningRune", "NatureRune", "RockRune", "WaterRune",
            "WindRune"),
        // Fields and falls stay silent by concept-doc rule; only the release and
        // the impact are audible, and each is one shared sound for every element.
        new("TransientLegacy",
            "CraterEmber", "ElectricField", "FireDrop", "FireField", "LeafField",
            "Leafair", "LightningDrop", "MeteorDrop", "MeteorShower", "MiniRock",
            "NatureDrop", "Overgrowth", "RainCloud", "RazorGale", "RockDrop",
            "RockRemnant", "RockRolling", "SandStorm", "TornadoStrike", "WaterField",
            "WindDrop"),
        new("TransientShot",
            "ChainLightning", "ElectricShot", "FireShot", "LeafShot", "MagmaFist",
            "TideCall", "WaterShot", "WindBlade"),
        new("TransientExplode",
            "ElectricExplode", "FireExplode", "LeafExplode", "MagmaExplosion",
            "RockExplode", "ShockOverload", "WaterExplode", "WaterExplosion",
            "WindExplode")
    };

    private static readonly string[] IntentionalSilentRuntimeTypes =
    {
        "ServedObjectHpBar",
        "Towerback"
    };

    [MenuItem("Tools/Sound/Create or Update Baseline Object SFX Catalog")]
    public static void CreateOrUpdate()
    {
        EnsureFolder(ConfigRoot);
        EnsureFolder(ProfileRoot);

        Dictionary<string, ObjectSfxProfile> profiles = CreateOrUpdateProfiles();
        WarnAboutStaleProfiles(profiles.Keys);

        ObjectSfxCatalog catalog = AssetDatabase.LoadAssetAtPath<ObjectSfxCatalog>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<ObjectSfxCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        SerializedObject serializedCatalog = new(catalog);
        SerializedProperty entries = serializedCatalog.FindProperty("entries");
        int entryCount = CountRuntimeTypes();
        entries.arraySize = entryCount;

        int index = 0;
        foreach (CatalogGroup group in CatalogGroups)
        {
            foreach (string runtimeType in group.RuntimeTypes)
            {
                WriteEntry(
                    entries.GetArrayElementAtIndex(index++),
                    runtimeType,
                    profiles[group.ProfileId],
                    false);
            }
        }

        foreach (string runtimeType in IntentionalSilentRuntimeTypes)
        {
            WriteEntry(entries.GetArrayElementAtIndex(index++), runtimeType, null, true);
        }

        serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        List<string> errors = ObjectSfxCatalogValidator.Validate();
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Baseline Object SFX catalog was created but validation found {errors.Count} error(s):\n" +
                string.Join("\n", errors));
        }

        Debug.Log($"Created or updated Object SFX catalog with {entryCount} explicit rows.");
    }

    private static Dictionary<string, ObjectSfxProfile> CreateOrUpdateProfiles()
    {
        var profiles = new Dictionary<string, ObjectSfxProfile>(StringComparer.Ordinal);
        foreach (ProfileDefinition definition in ProfileDefinitions)
        {
            string path = $"{ProfileRoot}/{definition.Id}.asset";
            ObjectSfxProfile profile = AssetDatabase.LoadAssetAtPath<ObjectSfxProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<ObjectSfxProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            SerializedObject serializedProfile = new(profile);
            serializedProfile.FindProperty("profileId").stringValue = definition.Id;
            serializedProfile.FindProperty("faction").enumValueIndex = (int)definition.Faction;
            serializedProfile.FindProperty("element").enumValueIndex = (int)definition.Element;
            serializedProfile.FindProperty("archetype").enumValueIndex = (int)definition.Archetype;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            profiles[definition.Id] = profile;
        }

        return profiles;
    }

    // Renaming a profile leaves the old asset behind, still referenced by nothing.
    // Deleting it is the author's call, so this only names them.
    private static void WarnAboutStaleProfiles(ICollection<string> knownProfileIds)
    {
        foreach (string guid in AssetDatabase.FindAssets(
                     $"t:{nameof(ObjectSfxProfile)}", new[] { ProfileRoot }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string id = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!knownProfileIds.Contains(id))
            {
                Debug.LogWarning(
                    $"Object SFX profile is no longer in the catalog and can be deleted: {path}.");
            }
        }
    }

    private static void WriteEntry(
        SerializedProperty entry,
        string runtimeType,
        ObjectSfxProfile profile,
        bool intentionalSilent)
    {
        entry.FindPropertyRelative("runtimeType").stringValue = runtimeType;
        entry.FindPropertyRelative("profile").objectReferenceValue = profile;
        entry.FindPropertyRelative("intentionalSilent").boolValue = intentionalSilent;
        entry.FindPropertyRelative("serverAlias").boolValue = false;
    }

    private static int CountRuntimeTypes()
    {
        int count = IntentionalSilentRuntimeTypes.Length;
        foreach (CatalogGroup group in CatalogGroups)
        {
            count += group.RuntimeTypes.Length;
        }

        return count;
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

    private sealed class CatalogGroup
    {
        public readonly string ProfileId;
        public readonly string[] RuntimeTypes;

        public CatalogGroup(string profileId, params string[] runtimeTypes)
        {
            ProfileId = profileId;
            RuntimeTypes = runtimeTypes;
        }
    }

    private sealed class ProfileDefinition
    {
        public readonly string Id;
        public readonly ObjectSfxFaction Faction;
        public readonly ObjectSfxElement Element;
        public readonly ObjectSfxArchetype Archetype;

        public ProfileDefinition(
            string id,
            ObjectSfxFaction faction,
            ObjectSfxElement element,
            ObjectSfxArchetype archetype)
        {
            Id = id;
            Faction = faction;
            Element = element;
            Archetype = archetype;
        }
    }
}
#endif
