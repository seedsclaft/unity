using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MainMenu;

namespace Ryneus
{
    public class MainMenuView : BaseView
    {
        [SerializeField] private BaseList stageList = null;
        [SerializeField] private StageInfoComponent component;
        [SerializeField] private OnOffButton nextStageButton;

        private new System.Action<MainMenuViewEvent> _commandData = null;

        public override void Initialize() 
        {
            base.Initialize();
            stageList.Initialize();
            SideMenuButton.SetCallHandler(() => 
            {
                CallSideMenu();
            });
            nextStageButton?.SetText(DataSystem.GetText(17010));
            nextStageButton?.SetCallHandler(() => 
            {
                var eventData = new MainMenuViewEvent(CommandType.NextStage);
                _commandData(eventData);
            });
            new MainMenuPresenter(this);
        }

        public void SetInitHelpText()
        {
            HelpWindow.SetHelpText(DataSystem.GetText(11040));
            HelpWindow.SetInputInfo("MAINMENU");
        }

        public void SetHelpWindow()
        {
            SetInitHelpText();
        }

        public void SetEvent(System.Action<MainMenuViewEvent> commandData)
        {
            _commandData = commandData;
        }
        
        public void SetStagesData(List<ListData> stages){
            stageList.SetData(stages);
            stageList.SetInputHandler(InputKeyType.Decide,() => CallMainMenuStage());
            stageList.SetInputHandler(InputKeyType.Option1,() => CommandOpenSideMenu());
            stageList.SetInputHandler(InputKeyType.Option2,() => CallStageRanking());
            stageList.SetSelectedHandler(() => UpdateMainMenuStage());
            for (var i = 0;i < stageList.ItemPrefabList.Count;i++)
            {
                if (i < stages.Count)
                {
                    var stageInfo = stageList.ItemPrefabList[i].GetComponent<MainMenuStage>();
                    stageInfo.SetRankingDetailHandler((a) => CallStageRanking(a));
                }
            }
            stageList.UpdateSelectIndex(0);
            SetInputHandler(stageList.GetComponent<IInputHandlerEvent>());
        }
        
        private void CallMainMenuStage()
        {
            var data = stageList.ListItemData<StageInfo>();
            if (data != null)
            {
                var eventData = new MainMenuViewEvent(CommandType.StageSelect)
                {
                    template = data.Id
                };
                _commandData(eventData);
            }
        }    
        
        private void CallStageRanking(int stageId = -1)
        {
            var data = stageList.ListItemData<StageInfo>();
            if (data != null)
            {
                var eventData = new MainMenuViewEvent(CommandType.Ranking);
                if (stageId > 0)
                {
                    eventData.template = stageId;
                } else
                {
                    eventData.template = data.Id;
                }
                _commandData(eventData);
            }
        }

        public void UpdateMainMenuStage()
        {
            var listData = stageList.ListData;
            if (listData != null)
            {
                var data = stageList.ListItemData<StageInfo>();
                //component.UpdateInfo(data);
            }
        }

        public void SetStageData(StageInfo stageInfo)
        {
            if (stageInfo != null)
            {
                component.UpdateInfo(stageInfo);
            }
        }

        private void CallSideMenu()
        {
            var eventData = new MainMenuViewEvent(CommandType.SelectSideMenu);
            _commandData(eventData);
        }
    }
}

namespace MainMenu
{
    public enum CommandType
    {
        None = 0,
        TacticsCommand,
        TacticsCommandClose,
        NextStage = 100,
        StageSelect = 101,
        Option = 103,
        Ranking = 104,
        SelectSideMenu,
    }
}
public class MainMenuViewEvent
{
    public CommandType commandType;
    public object template;

    public MainMenuViewEvent(CommandType type)
    {
        commandType = type;
    }
}