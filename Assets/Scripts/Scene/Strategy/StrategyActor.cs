using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Ryneus
{
    public class StrategyActor : ListItem ,IListViewItem  
    {   
        [SerializeField] private GameObject innerObj;
        [SerializeField] private ActorInfoComponent component;
        [SerializeField] private _2dxFX_Shiny_Reflect shinyReflect;
        [SerializeField] private Image bonusImage;
        [SerializeField] private Image shinyClip;

        private System.Action _callEvent = null;

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = (ActorInfo)ListData.Data;
            component.Clear();
            component.UpdateInfo(data,null);
        }

        public void StartResultAnimation(int animId,bool isBonus)
        {
            KillShinyReflect();
            var initPosy = (animId % 2 == 1) ? -80 : 80;
            innerObj.transform.DOLocalMoveY(initPosy,0.0f);
            var sequence = DOTween.Sequence()
                .Append(innerObj.transform.DOLocalMoveY(0,0.8f))
                .SetEase(Ease.OutQuart)
                .OnComplete(() => {
                    if (isBonus) 
                    {
                        StartBonusAnimation();
                    }
                    if (_callEvent != null) _callEvent();
                });
        }

        private void StartBonusAnimation()
        {
            var rand = Random.Range(1,100);
            bonusImage.transform.DOScaleY(0,0.0f);
            var sequence = DOTween.Sequence()
                .Append(bonusImage.transform.DOScaleY(1.5f,0.4f))
                .Join(bonusImage.DOFade(0.75f,0.1f))
                .Append(bonusImage.DOFade(0.0f,0.3f))
                .SetEase(Ease.OutQuart)
                .OnComplete(() => {
                    var sequence = DOTween.Sequence()
                    .SetDelay(rand * 0.01f)
                    .OnComplete(() => {
                        shinyReflect.enabled = true;
                    });
                });
        }

        public void SetShinyReflect(bool isEnable)
        {
            shinyReflect.enabled = isEnable;
            if (isEnable == false)
            {
                KillShinyReflect();
            }
        }

        public void SetEndCallEvent(System.Action callEvent)
        {
            _callEvent = callEvent;
        }

        public void KillShinyReflect()
        {
            shinyReflect.enabled = false;
            if (shinyClip.material != null)
            {
                shinyClip.material = null;
                shinyClip.SetMaterialDirty();
            }
        }
    }
}