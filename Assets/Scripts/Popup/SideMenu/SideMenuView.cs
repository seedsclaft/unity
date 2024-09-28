using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SideMenu;

namespace Ryneus
{
    public class SideMenuView : BaseView
    {
        [SerializeField] private BaseList sideMenuInfoList = null;
        [SerializeField] private Button closeButton = null;
        [SerializeField] private SideMenuAnimation sideMenuAnimation = null;
        private new System.Action<SideMenuViewEvent> _commandData = null;

        public SystemData.CommandData SideMenuCommand 
        { 
            get 
            {
                var listData = sideMenuInfoList.ListData;
                if (listData != null)
                {
                    return (SystemData.CommandData)listData.Data;
                }
                return null;
            }
        }

        public override void Initialize() 
        {
            base.Initialize();
            sideMenuInfoList.Initialize();
            closeButton.onClick.AddListener(() => 
            {
                BackEvent();
            });
            SetBaseAnimation(sideMenuAnimation);
            new SideMenuPresenter(this);
        }

        public void OpenAnimation()
        {
            sideMenuAnimation?.OpenAnimation(UiRoot.transform,null);
        }

        private void OnClickSideMenu()
        {
            var eventData = new SideMenuViewEvent(CommandType.SelectSideMenu);
            _commandData(eventData);
        }

        public void SetEvent(System.Action<SideMenuViewEvent> commandData)
        {
            _commandData = commandData;
        }
        
        public void SetSideMenuViewInfo(SideMenuViewInfo sideMenuViewInfo)
        {
            sideMenuInfoList.SetData(sideMenuViewInfo.CommandLists);
            sideMenuInfoList.SetInputHandler(InputKeyType.Decide,() =>
            {
                //sideMenuInfoList.Deactivate();
                OnClickSideMenu();
            });
            sideMenuInfoList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            SetInputHandler(sideMenuInfoList.gameObject);
        }

        public void ActivateSideMenu()
        {
            sideMenuInfoList.Activate();
        }
    }

    public class SideMenuViewInfo
    {
        public List<ListData> CommandLists;
        public System.Action EndEvent;
    }
}

namespace SideMenu
{
    public enum CommandType
    {
        None = 0,
        SelectSideMenu,
    }
}

public class SideMenuViewEvent
{
    public CommandType commandType;
    public object template;

    public SideMenuViewEvent(CommandType type)
    {
        commandType = type;
    }
}

