using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class GuideView : BaseView
    {
        [SerializeField] private BaseList helpTextList = null;
        [SerializeField] private Image guideImage = null;
        [SerializeField] private Button leftButton = null;
        [SerializeField] private Button rightButton = null;
        [SerializeField] private Button helpButton = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        private new System.Action<GuideViewEvent> _commandData = null;
        
        public override void Initialize() 
        {
            base.Initialize();
            SetBaseAnimation(popupAnimation);
            helpTextList.Initialize();
            leftButton.onClick.AddListener(() => OnClickLeft());
            rightButton.onClick.AddListener(() => OnClickRight());
            helpButton.onClick.AddListener(() => OnClickHelp());
            new GuidePresenter(this);
        }
        
        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform,null);
        }

        private void OnClickLeft()
        {
            if (!leftButton.gameObject.activeSelf) return;
            var eventData = new GuideViewEvent(Guide.CommandType.PageLeft);
            _commandData(eventData);
        }

        private void OnClickRight()
        {
            if (!rightButton.gameObject.activeSelf) return;
            var eventData = new GuideViewEvent(Guide.CommandType.PageRight);
            _commandData(eventData);
        }

        private void OnClickHelp()
        {
            var eventData = new GuideViewEvent(Guide.CommandType.CallHelp);
            _commandData(eventData);
        }

        public void SetLeftRight(bool left,bool right)
        {
            leftButton?.gameObject?.SetActive(left);
            rightButton?.gameObject?.SetActive(right);
        }

        public void SetGuide(string guideKey)
        {
            var eventData = new GuideViewEvent(Guide.CommandType.StartGuide)
            {
                template = guideKey
            };
            _commandData(eventData);
        }
        
        public void SetEvent(System.Action<GuideViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetGuideImage(Sprite guideSprite)
        {
            guideImage.sprite = guideSprite;
        }

        public void SetHelpText(List<ListData> helpTexts)
        {
            helpTextList.SetData(helpTexts);
        }
    }

    namespace Guide
    {
        public enum CommandType
        {
            None = 0,
            PageLeft,
            PageRight,
            StartGuide,
            CallHelp,
        }
    }

    public class GuideViewEvent
    {
        public Guide.CommandType commandType;
        public object template;

        public GuideViewEvent(Guide.CommandType type)
        {
            commandType = type;
        }
    }
}