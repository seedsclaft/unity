using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class ClearPartyView : BaseView
    {
        [SerializeField] private BaseList partyList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        private new System.Action<ClearPartyViewEvent> _commandData = null;
        private System.Action<SaveBattleInfo> _callEvent = null;
        
        public override void Initialize() 
        {
            base.Initialize();
            partyList.Initialize();
            SetBaseAnimation(popupAnimation);
            new ClearPartyPresenter(this);
            partyList.SetInputHandler(InputKeyType.Option1,() => CallCheckBattleReplay());
            partyList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            SetInputHandler(partyList.GetComponent<IInputHandlerEvent>());
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform,null);
        }

        public void SetBattleReplayEvent(System.Action<SaveBattleInfo> callEvent)
        {
            _callEvent = callEvent;
        }
        
        public void SetEvent(System.Action<ClearPartyViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetClearParty(List<ListData> partyLists)
        {
            partyList.SetData(partyLists);
            partyList.Activate();
            for (int i = 0; i < partyList.ItemPrefabList.Count;i++)
            {
                var listItem = partyList.ItemPrefabList[i].GetComponentInChildren<ClearPartyListItem>();
                listItem.SetBattleReplayHandler((a) => CallCheckBattleReplay(a));
            }
        }

        private void CallCheckBattleReplay(SaveBattleInfo saveBattleInfo = null)
        {
            var listData = partyList.ListData;
            if (listData != null)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                if (saveBattleInfo != null)
                {
                    _callEvent(saveBattleInfo);
                    return;
                }
                var data = (SaveBattleInfo)listData.Data;
                _callEvent(data);
            }
        }
    }
}
namespace ClearParty
{
    public enum CommandType
    {
        None = 0,
        DecideActor = 0,
    }
}

public class ClearPartyViewEvent
{
    public ClearParty.CommandType commandType;
    public object template;

    public ClearPartyViewEvent(ClearParty.CommandType type)
    {
        commandType = type;
    }
}