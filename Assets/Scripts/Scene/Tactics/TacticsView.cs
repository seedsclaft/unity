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
        [SerializeField] private TrainView trainView = null;
        [SerializeField] private BaseList tacticsCommandList = null;
        [SerializeField] private TacticsCharaLayer tacticsCharaLayer = null;
        [SerializeField] private TacticsSymbolList tacticsSymbolList = null;
        [SerializeField] private StageInfoComponent stageInfoComponent = null;
        [SerializeField] private AlcanaInfoComponent alcanaInfoComponent = null;
        [SerializeField] private BaseList symbolRecordList = null;
        [SerializeField] private BaseList parallelList = null;
        [SerializeField] private MagicList alcanaSelectList = null;

        [SerializeField] private TextMeshProUGUI turnText = null;
        private new System.Action<TacticsViewEvent> _commandData = null;
        [SerializeField] private TacticsAlcana tacticsAlcana = null;


        [SerializeField] private Button alcanaButton = null;
        [SerializeField] private Button stageHelpButton = null;
        [SerializeField] private Button commandHelpButton = null;
        
        [SerializeField] private GameObject backGround = null;
        [SerializeField] private Button symbolBackButton = null;
        public void SetActiveBackGround(bool isActive)
        {
            backGround.SetActive(isActive);
        }



        public int ParallelListIndex => parallelList.Index;
        public int RecordSeekIndex() 
        {
            var listData = symbolRecordList.ListData;
            if (listData != null)
            {
                var data = (List<SymbolInfo>)listData.Data;
                if (data != null && data.Count > 0)
                {
                    return data[0].StageSymbolData.Seek;
                }
            }
            return -1;
        }

        private bool _initRecordDisplay = false;

        public override void Initialize()
        {
            base.Initialize();


            tacticsCommandList.Initialize();
            tacticsAlcana.gameObject.SetActive(false);
            alcanaButton.onClick.AddListener(() => CallAlcanaCheck());
            
            tacticsSymbolList.Initialize();

            SideMenuButton.onClick.AddListener(() => 
            {
                var eventData = new TacticsViewEvent(CommandType.SelectSideMenu);
                _commandData(eventData);
            });
            stageHelpButton.onClick.AddListener(() => 
            {
                var eventData = new TacticsViewEvent(CommandType.StageHelp);
                _commandData(eventData);
            });
            commandHelpButton.onClick.AddListener(() => 
            {
                var eventData = new TacticsViewEvent(CommandType.CommandHelp);
                _commandData(eventData);
            });
            symbolBackButton?.onClick.AddListener(() => 
            {
                var eventData = new TacticsViewEvent(CommandType.CancelRecordList);
                _commandData(eventData);
            });
            symbolRecordList.Initialize();
            parallelList.Initialize();
            trainView.Initialize(base._commandData);
            trainView.SetHelpWindow(HelpWindow);
            alcanaSelectList.Initialize();
            new TacticsPresenter(this);
            trainView.HideSelectCharacter();
            HideSymbolRecord();
            HideParallelList();
            alcanaSelectList.Hide();
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

        public void SetTacticsCommand(List<ListData> menuCommands)
        {
            tacticsCommandList.SetData(menuCommands);
            tacticsCommandList.SetInputHandler(InputKeyType.Decide,() => CallTacticsCommand());
            tacticsCommandList.SetInputHandler(InputKeyType.Option1,() => CommandOpenSideMenu());
            tacticsCommandList.SetSelectedHandler(() => UpdateHelpWindow());
            SetInputHandler(tacticsCommandList.GetComponent<IInputHandlerEvent>());
            UpdateHelpWindow();
        }
        
        private void CallTacticsCommand()
        {
            var listData = tacticsCommandList.ListData;
            if (listData != null && listData.Enable)
            {
                var commandData = (SystemData.CommandData)listData.Data;
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var eventData = new TacticsViewEvent(CommandType.TacticsCommand);
                eventData.template = commandData.Id;
                _commandData(eventData);
            }
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
                SetHelpText(commandData.Help);
            }
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

        public void ChangeSymbolBackCommandActive(bool IsActive)
        {
            symbolBackButton?.gameObject.SetActive(IsActive);
        }

        private void OnClickBack()
        {
            var eventData = new TacticsViewEvent(CommandType.Back);
            _commandData(eventData);
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

        public void SetAlcanaInfo(List<SkillInfo> skillInfos)
        {
            alcanaInfoComponent.UpdateInfo(skillInfos);
        }


        public void SetTacticsCharaLayer(List<ActorInfo> actorInfos)
        {
            tacticsCharaLayer.SetData(actorInfos);
        }

        public void SetSymbols(List<ListData> symbolInfos)
        {
            tacticsSymbolList.SetData(symbolInfos);
            tacticsSymbolList.SetInfoHandler((a) => OnClickEnemyInfo(a));
            HideRecordList();
        }

        public void CommandRefresh()
        {
            trainView.CommandRefresh();
        }

        public void SetEvaluate(int partyEvaluate,int troopEvaluate)
        {
            trainView.SetEvaluate(partyEvaluate,troopEvaluate);
        }


        public void ShowSelectCharacter(List<ListData> tacticsActorInfo,TacticsCommandData tacticsCommandData)
        {
            trainView.ShowSelectCharacter(tacticsActorInfo,tacticsCommandData);
        }

        public void HideSelectCharacter()
        {
            trainView.HideSelectCharacter();
        }

        public void ShowSelectCharacterCommand()
        {
            trainView.ShowSelectCharacterCommand();
        }

        public void HideSelectCharacterCommand()
        {
            trainView.HideSelectCharacterCommand();
        }

        public void ShowConfirmCommand()
        {
            trainView.ShowConfirmCommand();
        }
        
        public void HideConfirmCommand()
        {
            trainView.HideConfirmCommand();
        }

        public void ShowCharacterDetail(ActorInfo actorInfo,List<ActorInfo> party)
        {
            trainView.ShowCharacterDetail(actorInfo,party);
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
            symbolRecordList.SetSelectedHandler(() => 
            {
                foreach (var SymbolRecordData in symbolRecordList.GetComponentsInChildren<SymbolRecordData>())
                {
                    SymbolRecordData.UpdateSelect(symbolRecordList.Index);
                }
            });
        }
        
        public void SetPositionSymbolRecords(List<ListData> symbolInfos)
        {
            if (_initRecordDisplay == false)
            {
                symbolRecordList.UpdateScrollRect(symbolInfos.FindIndex(a => a.Selected));
                _initRecordDisplay = true;
            }
        }

        private void OnClickSymbol(int seek)
        {
            if (symbolRecordList.ScrollRect.enabled == false) return;
            var eventData = new TacticsViewEvent(CommandType.SelectRecord);
            eventData.template = seek;
            _commandData(eventData);
        }

        public void ShowSymbolRecord()
        {
            symbolRecordList.gameObject.SetActive(true);
        }

        public void HideSymbolRecord()
        {
            symbolRecordList.gameObject.SetActive(false);
        }

        private void CallBattleEnemy()
        {
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
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
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
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
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

        public void HideRecordList()
        {
            tacticsSymbolList.gameObject.SetActive(false);
            SetHelpInputInfo("TACTICS");
            symbolRecordList.ScrollRect.enabled = true;
        }

        public void SetTurns(int turns)
        {
            turnText.text = (turns).ToString();
        }

        public void ActivateTacticsCommand()
        {
            trainView.ActivateTacticsCommand();
        }

        public void DeactivateTacticsCommand()
        {
            trainView.DeactivateTacticsCommand();
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

        public void HideAlcanaList()
        {
            alcanaSelectList.Hide();
        }

        public void SetAlcanaSelectInfos(List<ListData> skillInfos)
        {
            SetBackEvent(() => {});
            alcanaSelectList.SetData(skillInfos);
            alcanaSelectList.SetInputHandler(InputKeyType.Decide,() => {
                if (AlcanaSelectSkillInfo() != null)
                {
                    var eventData = new TacticsViewEvent(CommandType.SelectAlcanaList);
                    eventData.template = AlcanaSelectSkillInfo();
                    _commandData(eventData);
                }
            });
            alcanaSelectList.Show();
        }

        public SkillInfo AlcanaSelectSkillInfo() 
        {
            var listData = alcanaSelectList.ListData;
            if (listData != null)
            {
                return (SkillInfo)listData.Data;
            }
            return null;
        }
    }
}