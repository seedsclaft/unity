using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AnimationUtility
{
    public static void LocalMoveToTransform(GameObject target,Vector3 from,Vector3 to,float duration)
    {
        target.GetComponent<RectTransform>().localPosition = from;
        DOTween.Sequence()
            .Append(target.transform.DOLocalMove(to,duration))
            .SetEase(Ease.OutQuart);
    }

    public static void LocalMoveToLoopTransform(GameObject target,Vector3 from,Vector3 to,float duration)
    {
        target.GetComponent<RectTransform>().localPosition = from;
        DOTween.Sequence()
            .Append(target.transform.DOLocalMove(to,duration))
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1,LoopType.Yoyo);
    }

    public static void AlphaToTransform(CanvasGroup canvasGroup,float from,float to,float duration)
    {
        canvasGroup.alpha = from;
        DOTween.Sequence()
            .Append(canvasGroup.DOFade(to,duration))
            .SetEase(Ease.OutQuart);
    }
}
