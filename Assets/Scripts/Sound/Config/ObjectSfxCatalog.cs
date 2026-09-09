using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sound.Config
{
    [Serializable]
    public class ObjectSfxCatalogEntry
    {
        [SerializeField] private string runtimeType;
        [SerializeField] private ObjectSfxProfile profile;
        [SerializeField] private ObjectSfxProfile signature;
        [SerializeField] private bool intentionalSilent;
        [SerializeField] private bool serverAlias;

        public string RuntimeType => runtimeType;
        public ObjectSfxProfile Profile => profile;
        public ObjectSfxProfile Signature => signature;
        public bool IntentionalSilent => intentionalSilent;
        public bool ServerAlias => serverAlias;
    }

    [CreateAssetMenu(fileName = "ObjectSfxCatalog", menuName = "Sound/Object SFX Catalog")]
    public class ObjectSfxCatalog : ScriptableObject
    {
        public const string ResourcesPath = "Sound/Config/ObjectSfxCatalog";

        [SerializeField] private List<ObjectSfxCatalogEntry> entries = new();

        public IReadOnlyList<ObjectSfxCatalogEntry> Entries => entries;

        public bool TryResolve(string runtimeType, out ObjectSfxProfile profile)
        {
            return TryResolve(runtimeType, out profile, out ObjectSfxProfile _);
        }

        public bool TryResolve(
            string runtimeType,
            out ObjectSfxProfile profile,
            out ObjectSfxProfile signature)
        {
            profile = null;
            signature = null;

            if (string.IsNullOrEmpty(runtimeType))
            {
                return false;
            }

            foreach (ObjectSfxCatalogEntry entry in entries)
            {
                if (!string.Equals(entry.RuntimeType, runtimeType, StringComparison.Ordinal))
                {
                    continue;
                }

                if (entry.IntentionalSilent)
                {
                    return true;
                }

                profile = entry.Profile;
                signature = entry.Signature;
                return true;
            }

            return false;
        }
    }
}
