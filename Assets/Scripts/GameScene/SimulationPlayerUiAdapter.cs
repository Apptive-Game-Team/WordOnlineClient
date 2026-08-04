using System.Collections.Generic;
using GameScene.Simulation.Rendering;
using GameScene.Simulation.Resources;
using GameScene.UI;
using UnityEngine;

namespace GameScene
{
    public sealed class SimulationPlayerUiAdapter : MonoBehaviour, ISimulationPlayerUi
    {
        [SerializeField] private GameSceneUIController controller;

        public void Configure(GameSceneUIController value) => controller = value;

        public void Render(PlayerResourceSnapshot snapshot)
        {
            if (snapshot == null || controller == null) return;
            controller.UpdateMana(snapshot.Mana);
            if (TimerController.Instance != null)
                TimerController.Instance.UpdateTimer((snapshot.RemainingFrames + 19) / 20);

            List<string> remaining = controller.GetAllCards();
            for (int index = 0; index < snapshot.Hand.Count; index++)
            {
                string card = snapshot.Hand[index];
                if (!remaining.Remove(card)) controller.AddCard(card);
            }
            for (int index = 0; index < remaining.Count; index++) controller.RemoveCard(remaining[index]);
        }
    }
}
