using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Ryneus
{
    public class TacticsSymbolListAnimation : BaseAnimation , IBaseAnimation
    {
        private bool _busy;
        public void OpenAnimation(Transform transform,System.Action endEvent,float duration = 0.1f)
        {
            _busy = true;
            transform.DOLocalMoveY(240,duration);
            BaseCanvas.alpha = 0;
            DOTween.Sequence()
                .Append(transform.DOLocalMoveY(0,duration))
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
