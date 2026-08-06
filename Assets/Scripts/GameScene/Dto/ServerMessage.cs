using Newtonsoft.Json;

namespace GameScene.Dto
{
    /// <summary>
    /// Base for every STOMP message the game server pushes. The <c>type</c> field selects the concrete
    /// message, so a payload can be read in a single pass.
    /// </summary>
    [JsonConverter(typeof(ServerMessageConverter))]
    public abstract class ServerMessage
    {
        public string type;
    }
}
