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
            .SetEase(Ease.InOutQuad);
    }
}
