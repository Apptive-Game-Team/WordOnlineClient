using System.Collections.Generic;
using Data.Adventures;
using Data.Adventures.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Adventures.Scenarios
{
    public class StagePanel : MonoBehaviour
    {
        [SerializeField] private List<ScenarioButton> buttons;
        
        private Stage stage;

        public void Setup(Stage stage)
        {
            this.stage = stage;

            if (stage.State == State.INACTIVE)
            {
                GetComponent<Image>().color = Color.gray;
            }
            
            PropagateSetup(stage);
        }
        
        private void PropagateSetup(Stage stage)
        {
            for (var i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                if (i < stage.Scenarios.Count)
                {
                    button.gameObject.SetActive(true);
                    button.Setup(stage.Scenarios[i]);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }
        }
    }
}