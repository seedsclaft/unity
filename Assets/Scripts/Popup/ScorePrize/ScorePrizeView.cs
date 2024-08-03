using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class ScorePrizeView : BaseView
    {
        [SerializeField] private BaseList scorePrizeList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        private new System.Action<ScorePrizeViewEvent> _commandData = null;
        private System.Action<int> _callEvent = null;
        
        public override void Initialize() 
        {
            base.Initialize();
            scorePrizeList.Initialize();
            SetBaseAnimation(popupAnimation);
            new ScorePrizePresenter(this);
            scorePrizeList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            SetInputHandler(scorePrizeList.GetComponent<IInputHandlerEvent>());
        }
        
        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform,null);
        }
        
        public void SetEvent(System.Action<ScorePrizeViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetScorePrize(List<ListData> ScorePrizes)
        {
            scorePrizeList.SetData(ScorePrizes);
            scorePrizeList.Activate();
        }
    }
}

namespace ScorePrize
{
    public enum CommandType
    {
        None = 0,
        DecideActor = 0,
    }
}

public class ScorePrizeViewEvent
{
    public ScorePrize.CommandType commandType;
    public object template;

    public ScorePrizeViewEvent(ScorePrize.CommandType type)
    {
        commandType = type;
    }
}