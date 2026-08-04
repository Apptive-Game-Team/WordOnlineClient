namespace GameScene.Dto
{
    /// <summary>One thing that happened during a frame. `type` tells how to read the ids.</summary>
    [System.Serializable]
    public class GameEventDto
    {
        public const string Hit = "hit";

        public string type;
        public int actorId;
        public int targetId;
    }
}
