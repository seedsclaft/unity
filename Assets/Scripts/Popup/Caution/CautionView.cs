using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class CautionView : BaseView
    {
        [SerializeField] private TextMeshProUGUI titleText = null;
        [SerializeField] private TextMeshProUGUI evaluateText = null;
        [SerializeField] private TextMeshProUGUI levelPlusText = null;
        [SerializeField] private CanvasGroup canvasGroup = null;

        public override void Initialize() 
        {
            base.Initialize();
        }
        
        public void SetTitle(string title)
        {
            ClearText();
            titleText?.SetText(title);
            canvasGroup.alpha = 1;
            AnimationUtility.AlphaToTransform(canvasGroup,
                1f,
                0,
                1,
                3);
        }

        public void SetLevelup(int from,int to)
        {
            ClearText();
            levelPlusText?.SetText("+" + (to-from).ToString());
            AnimationUtility.CountUpText(evaluateText,from,to);
            canvasGroup.alpha = 1;
            AnimationUtility.AlphaToTransform(canvasGroup,
                1f,
                0,
                1,
                3);
        }

        private void ClearText()
        {
            titleText.SetText("");
            evaluateText.SetText("");
            levelPlusText.SetText("");
        }
    }

    public class CautionInfo
    {
        private string _title = "";
        public string Title => _title;
        public void SetTitle(string title)
        {
            _title = title;
        }

        private int _from = 0;
        public int From => _from;
        private int _to = 0;
        public int To => _to;
        public void SetLevelUp(int from,int to)
        {
            _from = from;
            _to = to;
        }

    }
}