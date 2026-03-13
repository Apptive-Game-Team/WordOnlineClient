using System;
using Data.Adventures;
using Data.Adventures.Domain;
using UnityEngine;

namespace Adventures.Scenarios
{
    public class StageFactory : MonoBehaviour
    {
        [SerializeField] private GameObject stageParent;

        private Adventure adventure;

        private void Awake()
        {
            adventure = CurrentAdventure.Instance.Adventure;
            
            adventure.Stages.ForEach(stage =>
            {
                GameObject panel = Instantiate(stage.StagePanelPrefab, stageParent.transform);
                StagePanel stagePanel = panel.GetComponent<StagePanel>();
                stagePanel.Setup(stage);
            });
        }
    }
}