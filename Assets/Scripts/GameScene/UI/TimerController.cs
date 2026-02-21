using System;
using Global;
using TMPro;
using UnityEngine;

namespace GameScene.UI
{
    public class TimerController : LocalSingletonObject<TimerController>
    {
    
        [SerializeField] private TMP_Text timerText;
        public event Action<int> OnTimeUpdated;
        
        public void UpdateTimer(int remainingTime)
        {
            OnTimeUpdated?.Invoke(remainingTime);
            string timeText = $"{remainingTime / 60:D2}:{(remainingTime % 60):D2}";
            timerText.text = timeText;
        }
    }
}
