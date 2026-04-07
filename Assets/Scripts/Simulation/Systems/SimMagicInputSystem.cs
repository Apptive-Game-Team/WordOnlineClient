using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Processes player magic inputs from confirmedFrame data.
    /// Validates cards/mana, resolves magic recipe, executes the spell.
    /// </summary>
    public class SimMagicInputSystem
    {
        private readonly SimMagicRegistry _registry;
        private readonly Dictionary<string, Dictionary<string, Fix64>> _parameters;

        public SimMagicInputSystem(SimMagicRegistry registry,
            Dictionary<string, Dictionary<string, Fix64>> parameters)
        {
            _registry = registry;
            _parameters = parameters;
        }

        /// <summary>
        /// Process a single player's input for this frame.
        /// </summary>
        public void HandleInput(SimWorld world, Master master, SimPlayerData playerData,
            SimCardDeck cardDeck, List<SimCardType> cards, SimVector3 position)
        {
            if (cards == null || cards.Count == 0) return;

            // Validate cards available
            if (!playerData.UseCards(cards))
                return;

            // Parse magic
            var magic = _registry.Parse(cards);
            if (magic == null)
            {
                // Invalid recipe: return cards to deck
                cardDeck.ReturnCards(cards);
                return;
            }

            // Range check
            SimVector3 playerPos = SimGameConfig.GetPlayerPosition(master);
            string rangeKey = magic.MagicType.ToString().ToLower();
            Fix64 range = GetParam(rangeKey, "range");
            if (playerPos.Distance(position) > range)
                return;

            // Execute magic
            magic.Run(world, master, playerPos, position);
            cardDeck.ReturnCards(cards);
        }

        private Fix64 GetParam(string group, string key)
        {
            if (_parameters.TryGetValue(group, out var g) && g.TryGetValue(key, out var v))
                return v;
            return Fix64.FromInt(10); // default range
        }
    }
}
