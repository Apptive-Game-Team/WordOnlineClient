using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Deterministic card deck. Shuffled using SimRandom for reproducibility.
    /// </summary>
    public class SimCardDeck
    {
        private static readonly Fix64 CARD_DRAW_INTERVAL = Fix64.One;

        private readonly Stat _drawInterval = new Stat(CARD_DRAW_INTERVAL);
        private readonly Queue<SimCardType> _cards = new();

        public SimCardDeck(List<SimCardType> cards, SimRandom rng)
        {
            // Deterministic Fisher-Yates shuffle
            var list = new List<SimCardType>(cards);
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            foreach (var c in list)
                _cards.Enqueue(c);
        }

        public void DrawCard(SimPlayerData player, int frameNum)
        {
            int interval = SimMath.FloorToInt(Fix64.FromInt(SimGameConfig.FPS) * _drawInterval.Total);
            if (interval <= 0) interval = 1;
            if (frameNum % interval != 0) return;
            if (_cards.Count == 0 || player.Cards.Count >= SimPlayerData.MAX_CARD_NUM) return;

            var card = _cards.Dequeue();
            player.AddCard(card);
        }

        public void ReturnCards(List<SimCardType> cards)
        {
            foreach (var c in cards)
                _cards.Enqueue(c);
        }

        public void Fever()
        {
            _drawInterval.AddPercent(-Fix64.Half);
        }
    }
}
