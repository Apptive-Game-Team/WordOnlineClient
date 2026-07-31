using GameScene.ServedObjectComponent.OnAttack;
using UnityEngine;

namespace GameScene.ServedObjectComponent.Sound
{
    // Issue #377 Step 0: legacy per-prefab game sounds stay silent until an
    // approved replacement owns each event. Remove the ObjectSpawner call to
    // restore legacy audio.
    public static class LegacySfxMuter
    {
        public static void Mute(GameObject spawnedObject)
        {
            foreach (OnAttackSoundPlayer legacyPlayer in
                     spawnedObject.GetComponentsInChildren<OnAttackSoundPlayer>(true))
            {
                legacyPlayer.enabled = false;
            }

            foreach (AudioSource source in
                     spawnedObject.GetComponentsInChildren<AudioSource>(true))
            {
                source.playOnAwake = false;
                source.mute = true;
                source.Stop();
            }
        }
    }
}
