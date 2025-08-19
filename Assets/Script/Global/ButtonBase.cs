using UnityEngine;
using UnityEngine.UI;

public abstract class ButtonBase : MonoBehaviour
{
    private AudioSource audioSource = null;
    private void Start()
    {
        audioSource = SceneContext.Instance.gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = SceneContext.Instance.gameObject.AddComponent<AudioSource>();
        }
    }

    protected abstract void OnClickButton();

    public virtual void ButtonEvent()
    {
        PlayUISound();
        OnClickButton();
    }

    private void PlayUISound()
    {
        audioSource.clip = SoundAssets.ClickButton;
        audioSource.Play();
    }
}
