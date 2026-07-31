using System;
using UnityEngine;

namespace Sound.Config
{
    public enum ObjectSfxElement
    {
        Neutral,
        Fire,
        Water,
        Nature,
        Lightning,
        Rock,
        Wind
    }

    public enum ObjectSfxArchetype
    {
        Creature,
        Slime,
        Aerial,
        Building,
        Projectile,
        TransientSpell
    }

    [Serializable]
    public class ObjectSfxEventSlot
    {
        [SerializeField] private bool enabled;
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField, Range(0.5f, 2f)] private float pitch = 1f;

        public bool Enabled => enabled;
        public AudioClip Clip => clip;
        public float Volume => volume;
        public float Pitch => pitch;
    }

    [CreateAssetMenu(fileName = "ObjectSfxProfile", menuName = "Sound/Object SFX Profile")]
    public class ObjectSfxProfile : ScriptableObject
    {
        [SerializeField] private string profileId;
        [SerializeField] private ObjectSfxElement element;
        [SerializeField] private ObjectSfxArchetype archetype;
        [SerializeField] private ObjectSfxEventSlot spawn = new();
        [SerializeField] private ObjectSfxEventSlot movement = new();
        [SerializeField] private ObjectSfxEventSlot attack = new();
        [SerializeField] private ObjectSfxEventSlot hit = new();
        [SerializeField] private ObjectSfxEventSlot heal = new();
        [SerializeField] private ObjectSfxEventSlot death = new();
        [SerializeField, Min(0f)] private float movementCooldown = 0.45f;

        public string ProfileId => profileId;
        public ObjectSfxElement Element => element;
        public ObjectSfxArchetype Archetype => archetype;
        public ObjectSfxEventSlot Spawn => spawn;
        public ObjectSfxEventSlot Movement => movement;
        public ObjectSfxEventSlot Attack => attack;
        public ObjectSfxEventSlot Hit => hit;
        public ObjectSfxEventSlot Heal => heal;
        public ObjectSfxEventSlot Death => death;
        public float MovementCooldown => movementCooldown;
        public bool IsStatic => archetype == ObjectSfxArchetype.Building;
        public bool IsProjectileOrTransient =>
            archetype == ObjectSfxArchetype.Projectile ||
            archetype == ObjectSfxArchetype.TransientSpell;
    }
}
