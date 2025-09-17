using DG.Tweening;
using UnityEngine;

public static class DOTweenAction
{
    public static void DOBounce(Transform tr, Vector3 originScale, float squashScale, float bounceScale, float duration)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(tr.DOScale(new Vector3(originScale.x * squashScale, originScale.y * bounceScale, originScale.z),
                duration / 2).SetEase(Ease.OutQuad))
            .Append(tr.DOScale(originScale, duration / 2).SetEase(Ease.InQuad));
    }

    public static void DOSwing(Transform tr, float angle, float duration)
    {
        Sequence seq = DOTween.Sequence();
        DOTween.Sequence()
            .Append(tr.DOLocalRotate(new Vector3(0, 0, +angle), duration * 0.2f, RotateMode.Fast)
                .SetEase(Ease.OutBack))
            .AppendInterval(duration * 0.4f)
            .Append(tr.DOLocalRotate(new Vector3(0, 0, -angle), duration * 0.2f, RotateMode.Fast)
                .SetEase(Ease.OutCubic))
            .Append(tr.DOLocalRotate(new Vector3(0, 0, 0), duration * 0.2f, RotateMode.Fast)
                .SetEase(Ease.OutQuad));
    }
}
