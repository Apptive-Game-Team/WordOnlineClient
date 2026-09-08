namespace Data
{
    /// <summary>
    /// 마법의 원소. 게임 서버 com.wordonline.server.game.domain.magic.ElementType 과
    /// 선언 순서를 맞춘다. <c>MagicBookScene.ElementChartView</c> 가 이 순서로 7x7 상성표를
    /// 인덱싱하므로 값을 재배열하면 상성표가 어긋난다.
    /// </summary>
    public enum ElementType
    {
        None,
        Fire,
        Water,
        Nature,
        Lightning,
        Rock,
        Wind,
    }
}
