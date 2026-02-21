using Scripts.Data.Sound;
using Scripts.Global;
using UnityEngine;

namespace Scripts.GameScene.ServedObjectComponent.OnAttack
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
                WDebug.Log($"[{gameObject.name}] AudioSource가 없어서 새로 추가합니다.");
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            audioSource.volume = audioSource.volume * SoundData.gameVolume / 100f;

            
            if (servedObject == null)
            {
                servedObject = transform.parent.GetComponentInChildren<ServedObject>();
            }
            
            if (servedObject == null)
            {
                WDebug.LogWarning($"[{transform.parent.name}] ServedObject를 찾지 못했습니다! (GetComponentInChildren 실패)");
                return; 
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
