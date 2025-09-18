using DG.Tweening;
using UnityEngine;

public static class DOTweenAction
{
    public struct BounceParameters
    {
        public Vector3 originScale;
        public float squashScale;
        public float bounceScale;
        public float duration;
    }
    public struct SwingParameters
    {
        public float angle;
        public float duration;
    }
    
    private static BounceParameters _mobBounceParam = new BounceParameters
    {
        originScale = Vector3.one,
        squashScale = 0.8f,
        bounceScale = 1.2f,
        duration    = 0.2f
    };
    private static BounceParameters _mobCrawlParam = new BounceParameters
    {
        originScale = Vector3.one,
        squashScale = 0.7f,
        bounceScale = 1.3f,
        duration    = 0.5f
    };
    
    private static SwingParameters _mobAttackParam = new ()
    {
        angle = 30f,
        duration  = 0.4f
    };

    private static SwingParameters _playerCardUseParam = new()
    {

        angle = 25f,
        duration = 0.5f
    };
    
    public static void Bounce(Transform tr, Vector3 originScale, float squashScale, float bounceScale, float duration)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(tr.DOScale(new Vector3(originScale.x * squashScale, originScale.y * bounceScale, originScale.z),
                duration / 2).SetEase(Ease.OutQuad))
            .Append(tr.DOScale(originScale, duration / 2).SetEase(Ease.InQuad));
    }

    public static void Crawl(Transform tr, Vector3 originScale, float stretchScale, float squashScale, float duration)
    {
            Sequence seq = DOTween.Sequence();
            seq.Append(tr.DOScale(new Vector3(originScale.x * stretchScale, originScale.y * squashScale, originScale.z),
                    duration / 2).SetEase(Ease.OutQuad))
                .Append(tr.DOScale(originScale, duration / 2).SetEase(Ease.InOutQuad));
            seq.SetLoops(-1);
    }

    //Has ZVisual Issue
    public static void Hover(Transform tr, float zPos, float duration)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(tr.DOMoveZ(zPos, duration/2 ).SetEase(Ease.InOutSine))
            .Append(tr.DOMoveZ(0, duration/2).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    public static void Swing(Transform tr, float angle, float duration)
    {
        Sequence seq = DOTween.Sequence();
        DOTween.Sequence()
            .Append(tr.DOLocalRotate(new Vector3(0, 0, +angle), duration * 0.3f, RotateMode.Fast)
                .SetEase(Ease.OutBack))
            .AppendInterval(duration * 0.3f)
            .Append(tr.DOLocalRotate(new Vector3(0, 0, -angle), duration * 0.2f, RotateMode.Fast)
                .SetEase(Ease.OutCubic))
            .Append(tr.DOLocalRotate(new Vector3(0, 0, 0), duration * 0.2f, RotateMode.Fast)
                .SetEase(Ease.OutQuad));
    }
    
    public static void Rotate(Transform tr, float angle, float duration, RotateMode mode = RotateMode.Fast)
    {
        Sequence seq = DOTween.Sequence();
        DOTween.Sequence()
            .Append(tr.DOLocalRotate(new Vector3(0, 0, angle), duration, mode)
                .SetEase(Ease.OutBack));
    }

    public static void BounceMob(Transform tr)
    {
        Bounce(tr, _mobBounceParam.originScale, _mobBounceParam.squashScale,_mobBounceParam.bounceScale, _mobBounceParam.duration);
    }

    public static void CrawlMob(Transform tr)
    {
        Crawl(tr, _mobCrawlParam.originScale, _mobCrawlParam.bounceScale, _mobCrawlParam.squashScale, _mobCrawlParam.duration);
    }
    
    public static void SwingMobAttack(Transform tr)
    {
        Swing(tr, _mobAttackParam.angle, _mobAttackParam.duration);
    }

    public static void RotatePlayerUseCard(Transform tr)
    {
        Bounce(tr, _mobBounceParam.originScale, _mobBounceParam.squashScale,_mobBounceParam.bounceScale, _mobBounceParam.duration);
        Rotate(tr, _playerCardUseParam.angle, _playerCardUseParam.duration, RotateMode.LocalAxisAdd);
    }
    public static void RotatePlayerCancelCard(Transform tr)
    {
        Rotate(tr, -_playerCardUseParam.angle, _playerCardUseParam.duration, RotateMode.LocalAxisAdd);
    }
    public static void RotatePlayerUseMagic(Transform tr)
    {
        Rotate(tr, 0, _playerCardUseParam.duration);
    }
}
