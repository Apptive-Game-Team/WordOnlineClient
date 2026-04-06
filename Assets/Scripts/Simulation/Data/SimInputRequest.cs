using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// A player's input for a single frame.
    /// Deserialized from the server's confirmedFrame.inputs map.
    /// </summary>
    public class SimInputRequest
    {
        public List<SimCardType> Cards;
        public SimVector3 Position;
        public int Id;
    }
}
