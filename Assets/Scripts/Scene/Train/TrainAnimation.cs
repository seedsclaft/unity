using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Ryneus
{
    public class TrainAnimation : BaseAnimation , IBaseAnimation
    {
        [SerializeField] private CanvasGroup detailCanvas = null;
        [SerializeField] private CanvasGroup characterListCanvas = null;
        private bool _busy;
        public void OpenAnimation(Transform transform,System.Action endEvent,float duration = 0.1f)
        {
            /*
            _busy = true;
            transform.DOLocalMoveX(20,duration);
            detailCanvas.alpha = 0.8f;
            DOTween.Sequence()
                .Append(transform.DOLocalMoveX(0,duration))
                .Join(detailCanvas.DOFade(1,duration)
                .OnComplete(() => 
                {
                    _busy = false;
                    if (endEvent != null) endEvent();
                })
                .SetEase(Ease.InOutQuad));
                */
        }

        public void OpenCharacterListAnimation(Transform transform,System.Action endEvent,float duration = 0.08f)
        {
            _busy = true;
            //transform.DOScaleY(0,duration);
            characterListCanvas.alpha = 0;
            DOTween.Sequence()
                //.Append(transform.DOScaleY(1,duration))
                .Join(characterListCanvas.DOFade(1,duration)
                .OnComplete(() => 
                {
                    _busy = false;
                    endEvent?.Invoke();
                })
                .SetEase(Ease.InOutQuad));
        }
    }
}
