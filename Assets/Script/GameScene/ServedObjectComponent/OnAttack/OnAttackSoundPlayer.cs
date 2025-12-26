using Script.Data.Sound;
using UnityEngine;

namespace Script.GameScene.ServedObjectComponent.OnAttack
{
    public class OnAttackSoundPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private ServedObject servedObject;

        private void Start()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.volume = audioSource.volume * SoundData.gameVolume / 100f;

            if (servedObject == null)
            {
                servedObject = transform.parent.GetComponentInChildren<ServedObject>();
            }
            servedObject.OnAttack += PlayAttackSound;
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
