using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Ryneus
{
    public class BaseAnimation : MonoBehaviour
    {
        private static Sequence _sequence;
        public static void MoveAndFade(RectTransform rect,Image image,float moveX,float fade,float duration = 0.1f,System.Action endEvent = null)
        {
            _sequence = DOTween.Sequence()
                .Append(rect.DOLocalMoveX(moveX,duration))
                .Join(image.DOColor(new Color(255,255,255,fade),duration)
                .OnComplete(() => 
                {
                    if (endEvent != null) endEvent();
                })
                .SetEase(Ease.InOutQuad));
        }

        public static void Kill()
        {
            if (_sequence != null)
            {
                _sequence.Complete();
            }
        }
    }
}