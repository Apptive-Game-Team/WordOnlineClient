using Script.Data.Sound;
using UnityEngine;

namespace Script.GameScene.ServedObjectComponent.OnAttack
{
    public class OnAttackSoundPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioSource audioSource;
        private ServedObject servedObject;

        private void Start()
        {
            servedObject = transform.parent.GetComponentInChildren<ServedObject>();
            servedObject.OnAttack += PlayAttackSound;
            audioSource.volume = audioSource.volume * SoundData.gameVolume / 100f;
        }

        private void PlayAttackSound()
        {
            audioSource.clip = attackSound;
            audioSource.Play();
        }

        private void OnDestroy()
        {
            if (servedObject != null) 
            {
                servedObject.OnAttack -= PlayAttackSound;
            }
        }
    }
}
