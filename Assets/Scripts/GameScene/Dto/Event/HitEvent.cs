namespace GameScene.Dto.Event
{
    [System.Serializable]
    public class HitEvent : GameEvent
    {
        public int actorId;
        public int targetId;
    }
}
