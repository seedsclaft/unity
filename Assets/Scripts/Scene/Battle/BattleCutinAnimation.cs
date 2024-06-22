using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Ryneus
{
    public class BattleCutinAnimation : MonoBehaviour
    {
        [SerializeField] private GameObject backUnMask1;
        [SerializeField] private GameObject backUnMask2;
        [SerializeField] private GameObject backUnMask3;
        [SerializeField] private RawImage mainBack;
        [SerializeField] private Image actorMain;
        [SerializeField] private CanvasGroup actorCanvasGroup;
        [SerializeField] private GameObject skillUnMask;
        [SerializeField] private CanvasGroup skillCanvasGroup;
        [SerializeField] private CanvasGroup skillNameCanvasGroup;
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        
        public void Initialize() 
        {
            skillUnMask.transform.DOLocalMove(new Vector3(0,0,0),0);
            skillCanvasGroup.gameObject.transform.DOLocalMove(new Vector3(0,-144,0),0);
            skillCanvasGroup.alpha = 0;
            skillNameCanvasGroup.alpha = 0;
        }

        public void StartAnimation(BattlerInfo battlerInfo,SkillData skillData,float speedRate)
        {
            if (battlerInfo != null)
            {
                battlerInfoComponent.UpdateInfo(battlerInfo);
            }
            if (skillData != null)
            {
                skillInfoComponent.UpdateData(skillData.Id);
            }
            
            var time1 = 0f;
            var time2 = 0.2f / speedRate;
            var time3 = 2.0f / speedRate;
            var time4 = 0.3f / speedRate;

            var backUnMask1X = 150;
            backUnMask1.transform.DOLocalMove(new Vector3(backUnMask1X,0,0),time1);
            var unMask1 = DOTween.Sequence()
                .Append(backUnMask1.transform.DOLocalMove(new Vector3(backUnMask1X,-600,0),time2))
                .AppendInterval(time3)
                .Append(backUnMask1.transform.DOLocalMove(new Vector3(backUnMask1X,0,0),time4))
                .SetEase(Ease.InOutCubic);

            var backUnMask2X = -300;
            backUnMask2.transform.DOLocalMove(new Vector3(backUnMask2X,0,0),time1);
            var unMask2 = DOTween.Sequence()
                //.SetDelay(0.1f)
                .Append(backUnMask2.transform.DOLocalMove(new Vector3(backUnMask2X,540,0),time2))
                .AppendInterval(time3)
                .Append(backUnMask2.transform.DOLocalMove(new Vector3(backUnMask2X,20,0),time4))
                .SetEase(Ease.InOutCubic);

            backUnMask3.transform.DOLocalMove(new Vector3(1600,400,0),time1);
            var unMask3 = DOTween.Sequence()
                .Append(backUnMask3.transform.DOLocalMove(new Vector3(-1600,-400,0),time2))
                .SetEase(Ease.InOutCubic);

            actorMain.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-240,-240,0);
            actorCanvasGroup.alpha = 0;
            var delay = 0.1f / speedRate;
            var actor = DOTween.Sequence()
                .SetDelay(delay)
                .Append(actorMain.transform.DOLocalMove(new Vector3(-184,-240,0),time2 - delay))
                .Join(actorCanvasGroup.DOFade(1,time2 - delay))
                .Append(actorMain.transform.DOLocalMove(new Vector3(-184 + 24,-240,0),time3))
                .Append(actorMain.transform.DOLocalMove(new Vector3(1280,-144,0),time4))
                .Join(actorCanvasGroup.DOFade(0,time4))
                .SetEase(Ease.InOutCubic)
                .OnComplete(() => {
                    mainBack.gameObject.SetActive(false);
                    gameObject.SetActive(false);

                });
        


            skillUnMask.transform.DOLocalMove(new Vector3(0,0,0),time1);
            skillCanvasGroup.gameObject.transform.DOLocalMove(new Vector3(0,-144,0),time1);
            skillCanvasGroup.alpha = 0;
            skillNameCanvasGroup.alpha = 0;
            
            var skill = DOTween.Sequence()
                //.SetDelay(0.1f)
                .Append(skillUnMask.transform.DOLocalMove(new Vector3(-1280,-80,0),time2))
                .Join(skillCanvasGroup.DOFade(1,time2))
                .Append(skillNameCanvasGroup.DOFade(1,time3))
                .Append(skillCanvasGroup.gameObject.transform.DOLocalMove(new Vector3(-1280,-80 - 144,0),time4))
                .Join(skillCanvasGroup.DOFade(0,time4))
                .SetEase(Ease.InOutCubic);

            mainBack.gameObject.SetActive(true);
            gameObject.SetActive(true);
        }

        private void Update() {
            if (mainBack.gameObject.activeSelf)
            {
                var x = mainBack.uvRect.x;
                mainBack.uvRect = new Rect(x+0.1f,0,1,1);
            }
        }
    }
}