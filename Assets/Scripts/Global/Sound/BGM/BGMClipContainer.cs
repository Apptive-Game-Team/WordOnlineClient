using UnityEngine;

namespace Scripts.Global.Sound.BGM
{
    public class BGMClipContainer : MonoBehaviour
    {
        [SerializeField] private AudioClip bgmClip;

        public AudioClip GetBGMClip()
        {
            return bgmClip;
        }
    }
}