using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Ryneus
{
    public class SideMenuAnimation : BaseAnimation , IBaseAnimation
    {
        public void OpenAnimation(Transform transform,System.Action endEvent,float duration = 0.1f)
        {
            Busy = true;
            transform.DOLocalMoveX(240,duration);
            BaseCanvas.alpha = 0;
            DOTween.Sequence()
                .Append(transform.DOLocalMoveX(0,duration))
                .Join(BaseCanvas.DOFade(1,duration)
                .OnComplete(() => 
                {
                    Busy = false;
                    if (endEvent != null) endEvent();
                })
                .SetEase(Ease.InOutQuad));
        }
    }
}
