using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MainMenu;
using TMPro;

namespace Ryneus
{
    public class MainMenuView : BaseView
    {
        [SerializeField] private TrainView trainView = null;
        [SerializeField] private BaseList tacticsCommandList = null;
        [SerializeField] private BaseList stageList = null;
        [SerializeField] private StageInfoComponent component;
        [SerializeField] private TextMeshProUGUI numinous = null;
        [SerializeField] private TextMeshProUGUI totalScore = null;

        private new System.Action<MainMenuViewEvent> _commandData = null;

        public override void Initialize() 
        {
            base.Initialize();
            stageList.Initialize();
            SideMenuButton.onClick.AddListener(() => 
            {
                CallSideMenu();
            });
            tacticsCommandList.Initialize();
            trainView.Initialize(base._commandData);
            trainView.SetHelpWindow(HelpWindow);
            new MainMenuPresenter(this);
        }

        public void SetTacticsCommand(List<ListData> menuCommands)
        {
            tacticsCommandList.SetData(menuCommands);
            tacticsCommandList.SetInputHandler(InputKeyType.Decide,() => CallTacticsCommand());
            tacticsCommandList.SetInputHandler(InputKeyType.Option1,() => CommandOpenSideMenu());
            tacticsCommandList.SetSelectedHandler(() => UpdateHelpWindow());
            SetInputHandler(tacticsCommandList.GetComponent<IInputHandlerEvent>());
            UpdateHelpWindow();
        }
        
        public void CallTrainCommand(TacticsCommandType tacticsCommandType)
        {
            trainView.CallTrainCommand(tacticsCommandType);
        }
        
        private void UpdateHelpWindow()
        {
            var listData = tacticsCommandList.ListData;
            if (listData != null)
            {
                var commandData = (SystemData.CommandData)listData.Data;
                HelpWindow.SetHelpText(commandData.Help);
            }
        }

        private void CallTacticsCommand()
        {
            var listData = tacticsCommandList.ListData;
            if (listData != null && listData.Enable)
            {
                var commandData = (SystemData.CommandData)listData.Data;
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var eventData = new MainMenuViewEvent(CommandType.TacticsCommand);
                eventData.template = commandData.Id;
                _commandData(eventData);
            }
        }

        public void ShowCommandList()
        {
            //sideMenuList.gameObject.SetActive(true);
            tacticsCommandList.gameObject.SetActive(true);
        }

        public void HideCommandList()
        {
            //sideMenuList.gameObject.SetActive(false);
            tacticsCommandList.gameObject.SetActive(false);
        }
        
        public void ShowSelectCharacter(List<ListData> tacticsActorInfo,TacticsCommandData tacticsCommandData)
        {
            trainView.ShowSelectCharacter(tacticsActorInfo,tacticsCommandData);
        }

        public void HideSelectCharacter()
        {
            trainView.HideSelectCharacter();
        }


        public void ShowLeaningList(List<ListData> learnMagicList)
        {
            trainView.ShowLeaningList(learnMagicList);
        }

        public void SetNuminous(int value)
        {
            numinous.SetText(DataSystem.GetReplaceDecimalText(value));
        }

        public void SetTotalScore(int value)
        {
            totalScore.SetText(DataSystem.GetReplaceDecimalText(value));
        }

        public void SetInitHelpText()
        {
            HelpWindow.SetHelpText(DataSystem.GetText(11040));
            HelpWindow.SetInputInfo("MAINMENU");
        }

        public void SetHelpWindow(){
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
        
        private void CallMainMenuStage(){
            var listData = stageList.ListData;
            if (listData != null)
            {
                var eventData = new MainMenuViewEvent(CommandType.StageSelect);
                var data = (StageInfo)listData.Data;
                eventData.template = data.Id;
                _commandData(eventData);
            }
        }    
        
        private void CallStageRanking(int stageId = -1){
            var listData = stageList.ListData;
            if (listData != null)
            {
                var eventData = new MainMenuViewEvent(CommandType.Ranking);
                if (stageId > 0)
                {
                    eventData.template = stageId;
                } else
                {
                    var data = (StageInfo)listData.Data;
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
                var data = (StageInfo)listData.Data;
                component.UpdateInfo(data);
            }
        }

        private void OnClickOption(){
            var eventData = new MainMenuViewEvent(CommandType.Option);
            _commandData(eventData);
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
        StageSelect = 101,
        Option = 103,
        Ranking = 104,
        SelectSideMenu,
    }
}
public class MainMenuViewEvent
{
    public MainMenu.CommandType commandType;
    public object template;

    public MainMenuViewEvent(MainMenu.CommandType type)
    {
        commandType = type;
    }
}