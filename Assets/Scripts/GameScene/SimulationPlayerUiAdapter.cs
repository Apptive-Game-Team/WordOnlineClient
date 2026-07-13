using System.Collections.Generic;
using GameScene.Simulation.Rendering;
using GameScene.Simulation.Resources;
using UnityEngine;

namespace GameScene
{
    public sealed class SimulationPlayerUiAdapter : MonoBehaviour, ISimulationPlayerUi
    {
        [SerializeField] private GameSceneUIController controller;

        public void Render(PlayerResourceSnapshot snapshot)
        {
            if (snapshot == null || controller == null) return;
            controller.UpdateMana(snapshot.Mana);

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
