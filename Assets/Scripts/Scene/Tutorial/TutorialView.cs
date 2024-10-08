using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
namespace Ryneus
{
    public class TutorialView : BaseView
    {
        [SerializeField] private Image focusImage = null;
        [SerializeField] private Image focusBgImage = null;
        [SerializeField] private Image arrowImage = null;

        public void SeekFocusImage(StageTutorialData stageTutorialData)
        {
            ShowFocusImage(stageTutorialData);
        }

        public void ShowFocusImage(StageTutorialData stageTutorialData)
        {
            gameObject.SetActive(true);
            if (focusImage == null) return;
            var rect = focusImage.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(stageTutorialData.X,stageTutorialData.Y,0);
            rect.sizeDelta = new Vector3(stageTutorialData.Width,stageTutorialData.Height);
            var bgRect = focusBgImage.GetComponent<RectTransform>();
            bgRect.localPosition = new Vector3(stageTutorialData.X * -1,stageTutorialData.Y * -1,0);
        }

        public void HideFocusImage()
        {
            gameObject.SetActive(false);
        }
    }
}
*/