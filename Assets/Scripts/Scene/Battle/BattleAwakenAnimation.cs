using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Effekseer;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    public class BattleAwakenAnimation : MonoBehaviour
    {
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        [SerializeField] private Image actorMain;
        [SerializeField] private Image enemyMain;
        [SerializeField] private GameObject enemyMainObject;
        [SerializeField] private EffekseerEmitter emitter;
        [SerializeField] private CanvasGroup canvasGroup;
        public void StartAnimation(BattlerInfo battlerInfo,Sprite actorSprite,float speedRate)
        {
            canvasGroup.alpha = 0;
            emitter.transform.DOScaleY(2.0f,0);
            if (battlerInfo.IsActorView)
            {
                actorMain.sprite = actorSprite;
            } else
            {
                battlerInfoComponent.UpdateInfo(battlerInfo);
            }
            actorMain.gameObject.SetActive(battlerInfo.IsActorView);
            enemyMainObject.SetActive(!battlerInfo.IsActorView);
            StartEmitterAnimation(speedRate);
        }

        private async void StartEmitterAnimation(float speedRate)
        {
            var emit = emitter.Play();
            emit.speed = 0.8f;
            var time1 = 0.3f / speedRate;
            var time2 = 0.5f / speedRate;
            gameObject.SetActive(true);
            DOTween.Sequence()
                .Append(emitter.transform.DOScaleY(4f,time1))
                .SetEase(Ease.InOutQuad);
            DOTween.Sequence()
                .Append(canvasGroup.DOFade(1,time2))
                .SetEase(Ease.InOutCubic);
            await UniTask.DelayFrame((int)(48f / speedRate));
            //await UniTask.WaitUntil(() => !emit.exists);
            
            gameObject.SetActive(false);
        }
    }
}
