using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tactics;
using TMPro;

namespace Ryneus
{
    public class TacticsView : BaseView
    {
        [SerializeField] private BaseList tacticsCommandList = null;
        [SerializeField] private TacticsCharaLayer tacticsCharaLayer = null;


        [SerializeField] private BattleSelectCharacter battleSelectCharacter = null;
        [SerializeField] private TacticsSelectCharacter selectCharacter = null;

        
        [SerializeField] private TacticsSymbolList tacticsSymbolList = null;
        [SerializeField] private StageInfoComponent stageInfoComponent = null;
        [SerializeField] private AlcanaInfoComponent alcanaInfoComponent = null;
        [SerializeField] private BaseList symbolRecordList = null;
        [SerializeField] private BaseList parallelList = null;

        [SerializeField] private TextMeshProUGUI turnText = null;
        [SerializeField] private TextMeshProUGUI numinousText = null;
        private new System.Action<TacticsViewEvent> _commandData = null;
        [SerializeField] private TacticsAlcana tacticsAlcana = null;


        [SerializeField] private Button alcanaButton = null;
        [SerializeField] private Button stageHelpButton = null;
        [SerializeField] private Button commandHelpButton = null;
        
        [SerializeField] private GameObject backGround = null;
        public void SetActiveBackGround(bool isActive)
        {
            backGround.SetActive(isActive);
        }


        private CommandType _lastCallEventType = CommandType.None;

        public SkillInfo SelectMagic => battleSelectCharacter.ActionData;
        public int ParallelListIndex => parallelList.Index;

        private bool _initRecordDisplay = false;


        public override void Initialize()
        {
            base.Initialize();

            battleSelectCharacter.Initialize();
            SetInputHandler(battleSelectCharacter.GetComponent<IInputHandlerEvent>());
            InitializeSelectCharacter();

            tacticsAlcana.gameObject.SetActive(false);
            alcanaButton.onClick.AddListener(() => CallAlcanaCheck());
            
            tacticsCommandList.Initialize();
            tacticsSymbolList.Initialize();
            battleSelectCharacter.gameObject.SetActive(false);

            SideMenuButton.onClick.AddListener(() => {
                var eventData = new TacticsViewEvent(CommandType.SelectSideMenu);
                _commandData(eventData);
            });
            stageHelpButton.onClick.AddListener(() => {
                var eventData = new TacticsViewEvent(CommandType.StageHelp);
                _commandData(eventData);
            });
            commandHelpButton.onClick.AddListener(() => {
                var eventData = new TacticsViewEvent(CommandType.CommandHelp);
                _commandData(eventData);
            });
            symbolRecordList.Initialize();
            parallelList.Initialize();
            new TacticsPresenter(this);
            selectCharacter.gameObject.SetActive(false);
            HideSymbolRecord();
            HideParallelList();
        }

        public void StartAnimation()
        {
            /*
            var duration = 0.4f;
            var tacticsCommandListRect = tacticsCommandList.GetComponent<RectTransform>();
            AnimationUtility.LocalMoveToTransform(tacticsCommandListRect.gameObject,
                new Vector3(tacticsCommandListRect.localPosition.x + 960,tacticsCommandListRect.localPosition.y,0),
                new Vector3(tacticsCommandListRect.localPosition.x,tacticsCommandListRect.localPosition.y,0),
                duration);
            var numinousRect = numinousText.gameObject.transform.parent.GetComponent<RectTransform>();
            AnimationUtility.LocalMoveToTransform(numinousRect.gameObject,
                new Vector3(numinousRect.localPosition.x + 480,numinousRect.localPosition.y,0),
                new Vector3(numinousRect.localPosition.x,numinousRect.localPosition.y,0),
                duration);
            var borderRect = stageInfoComponent.gameObject.GetComponent<RectTransform>();
            AnimationUtility.LocalMoveToTransform(borderRect.gameObject,
                new Vector3(borderRect.localPosition.x - 480,borderRect.localPosition.y,0),
                new Vector3(borderRect.localPosition.x,borderRect.localPosition.y,0),
                duration);
                */
        }
        
        private void InitializeSelectCharacter()
        {
            battleSelectCharacter.SetInputHandlerAction(InputKeyType.Decide,() => CallSkillAlchemy());
            battleSelectCharacter.SetInputHandlerAction(InputKeyType.Cancel,() => OnClickBack());
            SetInputHandler(battleSelectCharacter.MagicList.GetComponent<IInputHandlerEvent>());
            battleSelectCharacter.HideActionList();
        }

        public void SetUIButton()
        {
            SetBackCommand(() => OnClickBack());
            tacticsSymbolList.SetInputHandler(InputKeyType.Decide,() => CallBattleEnemy());
            tacticsSymbolList.SetInputHandler(InputKeyType.Option1,() => OnClickEnemyInfo());
            tacticsSymbolList.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
            SetInputHandler(tacticsSymbolList.GetComponent<IInputHandlerEvent>());
            tacticsSymbolList.SetInputCallHandler();
        }

        private void OnClickBack()
        {
            if (_lastCallEventType != CommandType.None) return;
            var eventData = new TacticsViewEvent(CommandType.Back);
            _commandData(eventData);
            _lastCallEventType = eventData.commandType;
        }

        private void CancelSymbol()
        {
            var eventData = new TacticsViewEvent(CommandType.TacticsCommandClose);
            eventData.template = ConfirmCommandType.No;
            _commandData(eventData);
        }

        private void OnClickEnemyListClose()
        {
            if (_lastCallEventType != CommandType.None) return;
            var eventData = new TacticsViewEvent(CommandType.SymbolClose);
            _commandData(eventData);
            _lastCallEventType = eventData.commandType;
        }


        public void SetHelpWindow()
        {
            /*
            sideMenuList.SetHelpWindow(HelpWindow);
            sideMenuList.SetOpenEvent(() => {
                tacticsCommandList.Deactivate();
                sideMenuList.Activate();
            });
            sideMenuList.SetCloseEvent(() => {
                SetHelpInputInfo("TACTICS");
                UpdateHelpWindow();
                tacticsCommandList.Activate();
                sideMenuList.Deactivate();
            });
            */
        }

        public void SetEvent(System.Action<TacticsViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetStageInfo(StageInfo stageInfo)
        {
            stageInfoComponent.UpdateInfo(stageInfo);
        }

        public void SetAlcanaInfo(AlcanaInfo alcanaInfo)
        {
            alcanaInfoComponent.UpdateInfo(alcanaInfo);
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

        public void RefreshListData(ListData listData)
        {
            tacticsCommandList.RefreshListData(listData);
            tacticsCommandList.Refresh();
            UpdateHelpWindow();
            //tacticsCommandList.SelectEnableIndex();
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

        private void CallTacticsCommand()
        {
            if (_lastCallEventType != CommandType.None) return;
            var listData = tacticsCommandList.ListData;
            if (listData != null && listData.Enable)
            {
                var commandData = (SystemData.CommandData)listData.Data;
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var eventData = new TacticsViewEvent(CommandType.TacticsCommand);
                eventData.template = commandData.Id;
                _commandData(eventData);
                _lastCallEventType = eventData.commandType;
            }
        }

        public void SetSelectCharacter(List<ListData> actorInfos,List<ListData> confirmCommands)
        {
            selectCharacter.Initialize();
            selectCharacter.SetCharacterData(actorInfos);
            SetInputHandler(selectCharacter.GetComponent<IInputHandlerEvent>());
            selectCharacter.SetTacticsCommand(confirmCommands);
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Decide,() => CallActorTrain());
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Right,() => CallFrontBattleIndex());
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Left,() => CallBattleBackIndex());
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Cancel,() => CallTrainCommandCancel());
            selectCharacter.SetInputHandlerCommand(InputKeyType.Decide,() => CallTrainCommand());
            selectCharacter.SetInputHandlerCommand(InputKeyType.Cancel,() => CallTrainCommandCancel());
            SetInputHandler(selectCharacter.CharacterList.GetComponent<IInputHandlerEvent>());
            SetInputHandler(selectCharacter.CommandList.GetComponent<IInputHandlerEvent>());
            selectCharacter.CharacterList.SetSelectedHandler(() => {
                var listData = selectCharacter.CharacterData;
                if (listData != null)
                {
                    var data = (TacticsActorInfo)listData.Data;
                    var eventData = new TacticsViewEvent(CommandType.ChangeSelectTacticsActor);
                    eventData.template = data.ActorInfo.ActorId;
                    _commandData(eventData);
                }
            });
            HideSelectCharacter();

        }

        public void SetTacticsCharaLayer(List<ActorInfo> actorInfos)
        {
            tacticsCharaLayer.SetData(actorInfos);
        }

        public void SetSymbols(List<ListData> symbolInfos)
        {
            tacticsSymbolList.SetData(symbolInfos);
            tacticsSymbolList.SetInfoHandler((a) => OnClickEnemyInfo(a));
            HideSymbolList();
        }

        public void CommandRefresh()
        {
            selectCharacter.Refresh();
        }


        public void SetEvaluate(int partyEvaluate,int troopEvaluate)
        {
            selectCharacter.SetEvaluate(partyEvaluate,troopEvaluate);
        }

        private void CallActorTrain()
        {
            if (_lastCallEventType != CommandType.None) return;
            var listData = selectCharacter.CharacterData;
            if (listData != null)
            {
                var data = (TacticsActorInfo)listData.Data;
                var eventData = new TacticsViewEvent(CommandType.SelectTacticsActor);
                eventData.template = data;
                _commandData(eventData);
                _lastCallEventType = eventData.commandType;
            }
        }

        private void CallTrainCommand()
        {
            if (_lastCallEventType != CommandType.None) return;
            var commandData = selectCharacter.CommandData;
            if (commandData != null)
            {
                var data = (SystemData.CommandData)commandData.Data;
                var commandType = ConfirmCommandType.No;
                if (data.Key == "Yes")
                {
                    commandType = ConfirmCommandType.Yes;
                }
                var eventData = new TacticsViewEvent(CommandType.TacticsCommandClose);
                eventData.template = commandType;
                _commandData(eventData);
                _lastCallEventType = eventData.commandType;
            }
        }

        private void CallTrainCommandCancel()
        {
            if (_lastCallEventType != CommandType.None) return;
            var eventData = new TacticsViewEvent(CommandType.TacticsCommandClose);
            eventData.template = ConfirmCommandType.No;
            _commandData(eventData);
            _lastCallEventType = eventData.commandType;
        }

        public void ShowSelectCharacter(List<ListData> tacticsActorInfo,TacticsCommandData tacticsCommandData)
        {
            selectCharacter.SetTacticsCharacter(tacticsActorInfo);
            selectCharacter.SetTacticsCommandData(tacticsCommandData);
            selectCharacter.UpdateSmoothSelect();
            selectCharacter.gameObject.SetActive(true);
        }

        public void HideSelectCharacter()
        {
            battleSelectCharacter.gameObject.SetActive(false);
            selectCharacter.gameObject.SetActive(false); 
            SetHelpInputInfo("TACTICS");
        }

        public void ShowSelectCharacterCommand()
        {
            selectCharacter.ShowCharacterList();
        }

        public void HideSelectCharacterCommand()
        {
            selectCharacter.HideCharacterList();
        }

        public void ShowConfirmCommand()
        {
            selectCharacter.ShowCommandList();
        }
        
        public void HideConfirmCommand()
        {
            selectCharacter.HideCommandList();
        }

        public void ShowCharacterDetail(ActorInfo actorInfo,List<ActorInfo> party)
        {
            battleSelectCharacter.gameObject.SetActive(true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Magic,true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Detail,true);
            battleSelectCharacter.SelectCharacterTab(SelectCharacterTabType.Detail);
            battleSelectCharacter.SetActorInfo(actorInfo,party);
            battleSelectCharacter.SetSkillInfos(actorInfo.SkillActionList());
            SetHelpInputInfo("ALCHEMY_ATTRIBUTE");
        }

        public void ShowLeaningList(List<ListData> learnMagicList)
        {
            battleSelectCharacter.gameObject.SetActive(true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Magic,true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
            battleSelectCharacter.SelectCharacterTab(SelectCharacterTabType.Magic);
            battleSelectCharacter.SetSkillInfos(learnMagicList);
            battleSelectCharacter.ShowActionList();
            battleSelectCharacter.HideStatus();
            battleSelectCharacter.MagicList.Activate();
            SetHelpInputInfo("ALCHEMY_ATTRIBUTE");
        }

        public void HideAttributeList()
        {
            battleSelectCharacter.HideActionList();
            battleSelectCharacter.MagicList.Deactivate();
            
            SetHelpInputInfo("ALCHEMY");
        }

        public void SetSymbolRecords(List<ListData> symbolInfos)
        {
            if (symbolRecordList.ListDates.Count > 0)
            {
                return;
            }
            symbolRecordList.SetData(symbolInfos);
            var SymbolRecordDates = symbolRecordList.GetComponentsInChildren<SymbolRecordData>();
            foreach (var SymbolRecordData in SymbolRecordDates)
            {
                SymbolRecordData.SetSymbolItemCallHandler((a) => OnClickSymbol(a));
            }
        }
        
        public void SetPositionSymbolRecords(List<ListData> symbolInfos)
        {
            if (_initRecordDisplay == false)
            {
                symbolRecordList.UpdateScrollRect(symbolInfos.FindIndex(a => a.Selected));
                _initRecordDisplay = true;
            }
        }

        private void OnClickSymbol(SymbolInfo symbolInfo)
        {
            if (symbolRecordList.ScrollRect.enabled == false) return;
            if (symbolInfo.SymbolType != SymbolType.None)
            {
                var eventData = new TacticsViewEvent(CommandType.SelectRecord);
                eventData.template = symbolInfo;
                _commandData(eventData);
            }
        }

        public void ShowSymbolRecord()
        {
            symbolRecordList.gameObject.SetActive(true);
        }

        public void HideSymbolRecord()
        {
            symbolRecordList.gameObject.SetActive(false);
        }

        private void CallFrontBattleIndex()
        {
            if (_lastCallEventType != CommandType.None) return;
            var listData = selectCharacter.CharacterData;
            if (listData != null)
            {
                var data = (TacticsActorInfo)listData.Data;
                if (data.TacticsCommandType != TacticsCommandType.Paradigm)
                {
                    return;
                }
                var eventData = new TacticsViewEvent(CommandType.SelectFrontBattleIndex);
                eventData.template = data.ActorInfo.ActorId;
                _commandData(eventData);
                _lastCallEventType = eventData.commandType;
            }
        }

        private void CallBattleBackIndex()
        {
            if (_lastCallEventType != CommandType.None) return;
            var listData = selectCharacter.CharacterData;
            if (listData != null)
            {
                var data = (TacticsActorInfo)listData.Data;
                if (data.TacticsCommandType != TacticsCommandType.Paradigm)
                {
                    return;
                }
                var eventData = new TacticsViewEvent(CommandType.SelectBackBattleIndex);
                eventData.template = data.ActorInfo.ActorId;
                _commandData(eventData);
                _lastCallEventType = eventData.commandType;
            }
        }

        private void CallBattleEnemy()
        {
            if (_lastCallEventType != CommandType.None) return;
            if (tacticsSymbolList.IsSelectSymbol())
            {
                var listData = tacticsSymbolList.ListData;
                if (listData != null)
                {
                    var data = (SymbolInfo)listData.Data;
                    if (data != null && data.SymbolType != SymbolType.None)
                    {
                        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
                        var eventData = new TacticsViewEvent(CommandType.SelectSymbol);
                        eventData.template = data;
                        _commandData(eventData);
                        _lastCallEventType = eventData.commandType;
                    }
                }
            } else
            {
                var getItemInfo = tacticsSymbolList.GetItemInfo();
                if (getItemInfo != null && (getItemInfo.IsSkill() || getItemInfo.IsAttributeSkill()))
                {
                    var eventData = new TacticsViewEvent(CommandType.PopupSkillInfo);
                    eventData.template = getItemInfo;
                    _commandData(eventData);
                    _lastCallEventType = eventData.commandType;
                }
            }
        }

        private void OnClickEnemyInfo(int selectIndex = -1)
        {
            var listData = tacticsSymbolList.ListData;
            if (listData != null)
            {
                var data = (SymbolInfo)listData.Data;
                if (data != null && data.SymbolType != SymbolType.None)
                {
                    Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
                    var eventData = new TacticsViewEvent(CommandType.CallEnemyInfo);
                    eventData.template = data;
                    _commandData(eventData);
                }
            }
        }

        private void OnClickParallel()
        {
            var eventData = new TacticsViewEvent(CommandType.Parallel);
            _commandData(eventData);
        }

        private void CallSymbolRecord()
        {
            var listData = symbolRecordList.ListData;
            if (listData != null)
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var eventData = new TacticsViewEvent(CommandType.DecideRecord);
                _commandData(eventData);
            }
        }

        public void SetParallelCommand(List<ListData> commands)
        {
            parallelList.SetData(commands);
            parallelList.SetInputHandler(InputKeyType.Cancel,() => CallSymbolRecord());
            parallelList.SetInputHandler(InputKeyType.Decide,() => OnClickParallel());
        }

        public void ShowParallelList()
        {
            parallelList.gameObject.SetActive(true);
        }

        public void HideParallelList()
        {
            parallelList.gameObject.SetActive(false);
        }

        public void ShowSymbolList()
        {
            tacticsSymbolList.gameObject.SetActive(true);
            tacticsSymbolList.ResetInputFrame(1);
            SetHelpInputInfo("ENEMY_SELECT");
            symbolRecordList.ScrollRect.enabled = false;
        }

        public void HideSymbolList()
        {
            tacticsSymbolList.gameObject.SetActive(false);
            SetHelpInputInfo("TACTICS");
            symbolRecordList.ScrollRect.enabled = true;
        }

        public void SetTurns(int turns)
        {
            turnText.text = (turns).ToString();
        }
        
        public void SetNuminous(int numinous)
        {
            numinousText.SetText(DataSystem.GetReplaceDecimalText(numinous));
        }

        private void CallSkillAlchemy()
        {
            if (_lastCallEventType != CommandType.None) return;
            var listData = battleSelectCharacter.ActionData;
            if (listData != null && listData.Enable)
            {
                var eventData = new TacticsViewEvent(CommandType.SkillAlchemy);
                var data = (SkillInfo)listData;
                eventData.template = listData;
                _commandData(eventData);
                _lastCallEventType = eventData.commandType;
            }
        }

        public void ActivateCommandList()
        {
            tacticsCommandList.Activate();
        }

        public void DeactivateCommandList()
        {
            tacticsCommandList.Deactivate();
        }

        void LateUpdate() {
            if (_lastCallEventType != CommandType.None){
                _lastCallEventType = CommandType.None;
            }
        }

        public void ActivateTacticsCommand()
        {
            if (selectCharacter.CharacterList.gameObject.activeSelf) selectCharacter.CharacterList.Activate();
        }

        public void DeactivateTacticsCommand()
        {
            if (selectCharacter.CharacterList.gameObject.activeSelf) selectCharacter.CharacterList.Deactivate();
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
        
        public void StartAlcanaAnimation(System.Action endEvent)
        {
            tacticsAlcana.StartAlcanaAnimation(endEvent);
        }

        private void CallAlcanaCheck()
        {
            var eventData = new TacticsViewEvent(CommandType.AlcanaCheck);
            _commandData(eventData);
        }
    }
}