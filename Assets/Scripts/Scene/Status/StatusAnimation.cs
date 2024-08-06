using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Ryneus
{
    public class StatusAnimation : BaseAnimation , IBaseAnimation
    {
        private bool _busy;
        public void OpenAnimation(Transform transform,System.Action endEvent,float duration = 0.2f)
        {
            _busy = true;
            //transform.DOScale(0,duration);
            BaseCanvas.alpha = 0;
            DOTween.Sequence()
                //.Append(transform.DOScale(1,duration))
                .Join(BaseCanvas.DOFade(1,duration)
                .OnComplete(() => 
                {
                    _busy = false;
                    endEvent?.Invoke();
                })
                .SetEase(Ease.InOutQuad));
        }

        public void LeftAnimation(Transform transform,System.Action endEvent,float duration = 0.4f)
        {
            _busy = true;
            transform.DOLocalMoveX(-640,duration);
            DOTween.Sequence()
                .Append(transform.DOLocalMoveX(0,duration))
                .OnComplete(() => 
                {
                    _busy = false;
                    endEvent?.Invoke();
                })
                .SetEase(Ease.InOutQuad);
        }

        public void RightAnimation(Transform transform,System.Action endEvent,float duration = 0.4f)
        {
            _busy = true;
            transform.DOLocalMoveX(640,duration);
            DOTween.Sequence()
                .Append(transform.DOLocalMoveX(0,duration))
                .OnComplete(() => 
                {
                    _busy = false;
                    endEvent?.Invoke();
                })
                .SetEase(Ease.InOutQuad);
        }
    }
}
