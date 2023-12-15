using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BaseAnimation : MonoBehaviour
{
    public static void MoveAndFade(RectTransform rect,Image image,float moveX,float fade,float duration = 0.2f)
    {
        var sequence = DOTween.Sequence()
            .Append(rect.DOLocalMoveX(moveX,duration))
            .Join(image.DOFade(fade,duration)
            .SetEase(Ease.OutQuad));
    }
}
