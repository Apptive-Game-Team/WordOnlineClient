using Script.Global;
using TMPro;
using UnityEngine;

namespace Script.GameScene.UI
{
    public class TimerController : LocalSingletonObject<TimerController>
    {
    
        [SerializeField] private TMP_Text timerText;
        
        public void UpdateTimer(int remainingTime)
        {
            string timeText = $"{remainingTime / 60:D2}:{(remainingTime % 60):D2}";
            timerText.text = timeText;
        }
    }
}
