using Newtonsoft.Json;

namespace GameScene.Dto.Event
{
    /// <summary>
    /// Something that happened during a single frame and cannot be read off object state, such as
    /// who struck whom. The server tags each entry with a <c>type</c> discriminator.
    /// </summary>
    [JsonConverter(typeof(GameEventConverter))]
    public abstract class GameEvent
    {
        public string type;
    }
}
