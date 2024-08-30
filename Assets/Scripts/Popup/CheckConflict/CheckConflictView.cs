using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CheckConflict;

namespace Ryneus
{
    public class CheckConflictView : BaseView
    {
        [SerializeField] private BaseList resultList = null;
        [SerializeField] private BaseList mainActorList = null;
        [SerializeField] private OnOffButton mainActorButton = null;
        [SerializeField] private BaseList brunchActorList = null;
        [SerializeField] private OnOffButton brunchActorButton = null;
        [SerializeField] private Toggle mainToggle = null;
        [SerializeField] private Toggle brunchToggle = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        private new System.Action<CheckConflictViewEvent> _commandData = null;
        private System.Action<int> _callEvent = null;
        public bool IsCheckedMain => mainToggle.isOn;
        public bool IsCheckedBrunch => brunchToggle.isOn;
        
        public override void Initialize() 
        {
            base.Initialize();
            resultList.Initialize();
            mainActorList.Initialize();
            brunchActorList.Initialize();
            SetBaseAnimation(popupAnimation);
            new CheckConflictPresenter(this);
            resultList.SetInputHandler(InputKeyType.Cancel,() => {});
            resultList.SetInputHandler(InputKeyType.Decide,() => {});
            mainActorButton.SetCallHandler(() => 
            {
                var eventData = new CheckConflictViewEvent(CommandType.MainActorStatus);
                _commandData(eventData);
            });
            brunchActorButton.SetCallHandler(() => 
            {
                var eventData = new CheckConflictViewEvent(CommandType.BrunchActorStatus);
                _commandData(eventData);
            });
            mainToggle.onValueChanged.AddListener((bool a) => 
            {
                var eventData = new CheckConflictViewEvent(CommandType.MainToggle);
                eventData.template = a;
                _commandData(eventData);
            });
            brunchToggle.onValueChanged.AddListener((bool a) => 
            {
                var eventData = new CheckConflictViewEvent(CommandType.BrunchToggle);
                eventData.template = a;
                _commandData(eventData);
            });
            SetInputHandler(resultList.GetComponent<IInputHandlerEvent>());
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform,null);
        }
        
        public void CheckMainToggle(bool isChecked)
        {
            mainToggle.SetIsOnWithoutNotify(isChecked);
        }

        public void CheckBrunchToggle(bool isChecked)
        {
            brunchToggle.SetIsOnWithoutNotify(isChecked);
        }

        public void SetViewInfo(CheckConflictInfo characterListInfo)
        {
            _callEvent = characterListInfo.CallEvent;
        }
        
        public void SetEvent(System.Action<CheckConflictViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetCheckConflict(List<ListData> symbolRecordList)
        {
            resultList.SetData(symbolRecordList);
            resultList.Activate();
        }

        public void SetMainActorInfos(List<ListData> actorInfos)
        {
            mainActorList.SetData(actorInfos);
            mainActorList.Activate();
        }
        
        public void SetBrunchActorInfos(List<ListData> actorInfos)
        {
            brunchActorList.SetData(actorInfos);
            brunchActorList.Activate();
        }

        private void CallDecideActor()
        {
            var listData = resultList.ListData;
            if (listData != null)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var data = (ActorInfo)listData.Data;
                _callEvent(data.ActorId);
            }
        }
    }
}

namespace CheckConflict
{
    public enum CommandType
    {
        None = 0,
        MainActorStatus,
        BrunchActorStatus,
        MainToggle,
        BrunchToggle,
    }
}

public class CheckConflictViewEvent
{
    public CommandType commandType;
    public object template;

    public CheckConflictViewEvent(CheckConflict.CommandType type)
    {
        commandType = type;
    }
}