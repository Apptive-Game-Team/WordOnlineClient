using Global;
using Global.Sound;
using UnityEngine;

namespace GameScene.ServedObjectComponent.OnAttack
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
            
            // Attach instead of scaling the volume in place: the old code baked the
            // ratio into audioSource.volume, so it compounded and never followed
            // the slider afterwards.
            SoundVolumeSetter.Attach(audioSource, SoundVolumeSetter.SoundType.Game, audioSource.volume);


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
