using DG.Tweening;
using Script.Global.Sound.BGM;
using UnityEngine;

namespace Script.GameScene.UI
{
    public class FeverTimeController : MonoBehaviour
    {
        
        [SerializeField] private GameObject feverTimeEffect;
        
        private bool isFeverTime = false;

        public void StartFeverTime()
        {
            if (isFeverTime) return;
            isFeverTime = true;

            OnFeverTimeStart();
        }

        private void OnFeverTimeStart()
        {
            BGMPlayer.Instance.SetPitch(1.5f);
            feverTimeEffect.SetActive(true);
            feverTimeEffect.transform.localScale = Vector3.zero;
            feverTimeEffect.transform.DOScale(1f, 0.5f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => 
                    feverTimeEffect.transform.DOScale(0f, 0.5f)
                        .SetDelay(1f)
                        .OnComplete(() => feverTimeEffect.SetActive(false)));
            Destroy(gameObject, 3);
        }

        private void OnDestroy()
        {
            BGMPlayer.Instance.SetPitch(1.0f);
        }
    }
}