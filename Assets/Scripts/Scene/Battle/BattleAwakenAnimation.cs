using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Effekseer;
using Cysharp.Threading.Tasks;
using UtageExtensions;

namespace Ryneus
{
    public class BattleAwakenAnimation : MonoBehaviour
    {
        [SerializeField] private Image actorMain;
        [SerializeField] private EffekseerEmitter emitter;
        [SerializeField] private CanvasGroup canvasGroup;
        public async void StartAnimation(Sprite actorSprite,float speedRate)
        {
            canvasGroup.alpha = 0;
            emitter.transform.DOScaleY(2.0f,0);
            actorMain.sprite = actorSprite;
            emitter.Play();

            var time1 = 0.3f / speedRate;
            var time2 = 0.5f / speedRate;
            gameObject.SetActive(true);
            DOTween.Sequence()
                .Append(emitter.transform.DOScaleY(4f,time1))
                .SetEase(Ease.InOutQuad);
            DOTween.Sequence()
                .Append(canvasGroup.DOFade(1,time2))
                .SetEase(Ease.InOutCubic);
            await UniTask.DelayFrame((int)(52 / speedRate));
            gameObject.SetActive(false);
        }
    }
}
