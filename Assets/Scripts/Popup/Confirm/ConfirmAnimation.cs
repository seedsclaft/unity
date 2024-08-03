using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Ryneus
{
    public class ConfirmAnimation : BaseAnimation , IBaseAnimation
    {
        private bool _busy;
        public void OpenAnimation(Transform transform,System.Action endEvent,float duration = 0.1f)
        {
            _busy = true;
            transform.DOScaleY(0,duration);
            BaseCanvas.alpha = 0;
            DOTween.Sequence()
                .Append(transform.DOScaleY(1,duration))
                .Join(BaseCanvas.DOFade(1,duration)
                .OnComplete(() => 
                {
                    _busy = false;
                    if (endEvent != null) endEvent();
                })
                .SetEase(Ease.InOutQuad));
        }
    }
}
