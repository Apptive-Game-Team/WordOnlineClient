using DG.Tweening;
using Script.Global.Sound.BGM;
using UnityEngine;

namespace Script.GameScene.UI
{
    public class FeverTimeController : MonoBehaviour
    {
        private static readonly int FeverTime = 30;
        
        [SerializeField] private GameObject feverTimeEffect;
        
        private bool isFeverTime = false;
        
        private void Start()
        {
            TimerController.Instance.OnTimeUpdated += CheckFeverTime;
            feverTimeEffect.SetActive(false);
        }
        
        private void OnDestroy()
        {
            TimerController.Instance.OnTimeUpdated -= CheckFeverTime;
        }
        
        public void CheckFeverTime(int remainingTime)
        {
            if (isFeverTime) return;
            if (remainingTime <= FeverTime)
            {
                isFeverTime = true;

                OnFeverTimeStart();
            };
        }

        private void OnFeverTimeStart()
        {
            BGMPlayer.Instance.SetPitch(2f);
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
    }
}