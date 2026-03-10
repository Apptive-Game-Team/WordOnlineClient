using Data.Adventures;
using Global.Button;
using UnityEngine.UI;
using Data.Adventures.Domain;
using UnityEngine;

namespace Adventures.Scenarios
{
    public class ScenarioButton : ButtonBase
    {
        private Image icon;
        private Scenario _scenario;
        public long Id => _scenario.Id;

        private void Awake()
        {
            icon = GetComponent<Image>();
        }
        
        protected override void OnClickButton()
        {
            throw new System.NotImplementedException();
        }

        public void Setup(Scenario scenario)
        {
            _scenario = scenario;

            switch (_scenario.State)
            {
                case State.FINISHED:
                    icon.color = new Color(0.30f, 0.80f, 0.45f); // 비비드 그린
                    break;

                case State.ACTIVE:
                    icon.color = new Color(1.00f, 0.75f, 0.20f); // 비비드 옐로우/골드
                    break;

                case State.INACTIVE:
                    icon.color = new Color(0.55f, 0.60f, 0.70f); // 약간 진한 그레이블루
                    break;
            }
        }
    }
}
