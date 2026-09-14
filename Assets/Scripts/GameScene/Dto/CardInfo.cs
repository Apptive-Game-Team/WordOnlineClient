namespace GameScene.Dto
{
    [System.Serializable]
    public class CardInfo
    {
        // TODO(#576): 서버 frame 의 cards.added 가 마법 id 목록(long)이 된다. 그때 이 배열을 long[] 로 바꾸고
        // GameSceneUIController.AddCard 도 id 를 받도록 옮긴다.
        public string[] added; // 지금은 마법 이름 문자열 목록이다
    }
}
