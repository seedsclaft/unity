using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tactics;
using TMPro;

namespace Ryneus
{
    public class TacticsView : BaseView ,IInputHandlerEvent
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

        [SerializeField] private TextMeshProUGUI saveScoreText = null;
        private new System.Action<TacticsViewEvent> _commandData = null;
        [SerializeField] private TacticsAlcana tacticsAlcana = null;


        [SerializeField] private Button alcanaButton = null;
        [SerializeField] private Button stageHelpButton = null;
        [SerializeField] private Button scorePrizeButton = null;
        [SerializeField] private Button leftButton = null;
        [SerializeField] private Button rightButton = null;
        
        [SerializeField] private GameObject backGround = null;
        [SerializeField] private Button symbolBackButton = null;
        public void SetActiveBackGround(bool isActive)
        {
            backGround.SetActive(isActive);
        }
        [SerializeField] private _2dxFX_NoiseAnimated _2DxFX_NoiseAnimated = null;
        [SerializeField] private TextMeshProUGUI pastText = null;
        public void SetPastMode()
        {
            pastText?.gameObject.SetActive(true);
            //pastText?.SetText("");
            _2DxFX_NoiseAnimated.enabled = true;
        }


        public int ParallelListIndex => parallelList.Index;
        public int AlcanaListIndex => alcanaSelectList.Index;
        public SymbolResultInfo SymbolResultInfo() 
        {
            var listData = symbolRecordList.ListData;
            if (listData != null)
            {
                var data = (List<SymbolResultInfo>)listData.Data;
                if (data != null && data.Count > 0)
                {
                    return data[0];
                }
            }
            return null;
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
                CallSideMenu();
            });
            stageHelpButton.onClick.AddListener(() => 
            {
                var eventData = new TacticsViewEvent(CommandType.StageHelp);
                _commandData(eventData);
            });
            scorePrizeButton?.onClick.AddListener(() => 
            {
                var eventData = new TacticsViewEvent(CommandType.ScorePrize);
                _commandData(eventData);
            });
            tacticsSymbolList.Initialize(() => 
            {
                var eventData = new TacticsViewEvent(CommandType.CancelRecordList);
                _commandData(eventData);
            });
            symbolRecordList.Initialize();
            SetInputHandler(symbolRecordList.gameObject);
            symbolRecordList.SetInputHandler(InputKeyType.Decide,() => OnClickSymbol());
            symbolRecordList.SetInputHandler(InputKeyType.Cancel,() => 
            {
                var eventData = new TacticsViewEvent(CommandType.CancelSymbolRecord);
                _commandData(eventData);
            });

            parallelList.Initialize();
            trainView.Initialize(base._commandData);
            trainView.SetParentInputKeyActive((a,b) => UpdateChildInputKeyActive(a,b));
            trainView.SetHelpWindow(HelpWindow);
            alcanaSelectList.Initialize();
            leftButton.onClick.AddListener(() => OnClickLeft());
            rightButton.onClick.AddListener(() => OnClickRight());
            var presenter = new TacticsPresenter(this);
            trainView.HideSelectCharacter();
            HideSymbolRecord();
            HideParallelList();
            alcanaSelectList.Hide();
            presenter.CommandReturnStrategy();
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

        private void CallSideMenu()
        {
            SetBusyTrain(true);
            var eventData = new TacticsViewEvent(CommandType.SelectSideMenu);
            _commandData(eventData);
        }

        public void SetBusyTrain(bool busy)
        {
            trainView.SetBusy(busy);
        }


        public void SetTacticsCommand(List<ListData> menuCommands)
        {
            tacticsCommandList.SetData(menuCommands);
            tacticsCommandList.SetInputHandler(InputKeyType.Decide,() => CallSelectTacticsCommand());
            tacticsCommandList.SetInputHandler(InputKeyType.Option1,() => CommandOpenSideMenu());
            tacticsCommandList.SetSelectedHandler(() => UpdateHelpWindow());
            SetInputHandler(tacticsCommandList.GetComponent<IInputHandlerEvent>());
            UpdateHelpWindow();
        }
        
        private void CallSelectTacticsCommand()
        {
            var listData = tacticsCommandList.ListData;
            if (listData != null && listData.Enable)
            {
                var commandData = (SystemData.CommandData)listData.Data;
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var eventData = new TacticsViewEvent(CommandType.SelectTacticsCommand)
                {
                    template = commandData.Id
                };
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
            tacticsSymbolList.SetInputHandler(InputKeyType.Cancel,() => 
            {
                var eventData = new TacticsViewEvent(CommandType.CancelRecordList);
                _commandData(eventData);
            });
            SetInputHandler(tacticsSymbolList.GetComponent<IInputHandlerEvent>());
            tacticsSymbolList.SetInputCallHandler();
        }

        public void ChangeSymbolBackCommandActive(bool IsActive)
        {
            tacticsSymbolList.ChangeSymbolBackCommandActive(IsActive);
        }

        private void OnClickBack()
        {
            var eventData = new TacticsViewEvent(CommandType.Back);
            _commandData(eventData);
        }

        private void OnClickLeft()
        {
            if (!leftButton.gameObject.activeSelf) return;
            var eventData = new TacticsViewEvent(CommandType.NormalWorld);
            _commandData(eventData);
        }

        private void OnClickRight()
        {
            if (!rightButton.gameObject.activeSelf) return;
            var eventData = new TacticsViewEvent(CommandType.AnotherWorld);
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


        public void ShowSelectCharacter()
        {
            trainView.ShowTacticsCharacter();
        }

        public void HideSelectCharacter()
        {
            trainView.HideSelectCharacter();
        }

        public void ShowConfirmCommand()
        {
            trainView.ShowConfirmCommand();
        }
        
        public void ShowBattleReplay(bool isActive)
        {
            trainView.ShowBattleReplay(isActive);
        }

        public void ShowCharacterDetail(ActorInfo actorInfo,List<ActorInfo> party)
        {
            trainView.ShowCharacterDetail(actorInfo,party);
        }

        public void SetSymbolRecords(List<ListData> recordInfos)
        {
            if (symbolRecordList.ListDates.Count > 0)
            {
                return;
            }
            symbolRecordList.SetData(recordInfos,false,() => 
            {
                var SymbolRecordDates = symbolRecordList.GetComponentsInChildren<SymbolRecordData>();
                foreach (var SymbolRecordData in SymbolRecordDates)
                {
                    SymbolRecordData.SetSymbolItemCallHandler((a) => OnClickSymbol());
                }
                symbolRecordList.SetSelectedHandler(() => 
                {
                    // 消さないこと
                });
            });

        }
        
        public void SetPositionSymbolRecords(int firstRecordIndex)
        {
            if (_initRecordDisplay == false)
            {
                var selectIndex = firstRecordIndex;
                var resultIndex = symbolRecordList.DataCount - selectIndex;
                if (resultIndex < 0)
                {
                    resultIndex = 0;
                }
                symbolRecordList.UpdateSelectIndex(resultIndex - 1);
                symbolRecordList.UpdateScrollRect(resultIndex + 1);
                _initRecordDisplay = true;
            }
        }

        private void OnClickSymbol()
        {
            if (symbolRecordList.ScrollRect.enabled == false) return;
            var listData = symbolRecordList.ListData;
            if (listData != null)
            {
                var data = (List<SymbolResultInfo>)listData.Data;
                if (data[0].SymbolType != SymbolType.None)
                {
                    var eventData = new TacticsViewEvent(CommandType.SelectRecord)
                    {
                        template = data[0]
                    };
                    _commandData(eventData);
                }
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

        private void CallBattleEnemy()
        {
            if (tacticsSymbolList.IsSelectSymbol())
            {
                var listData = tacticsSymbolList.ListData;
                if (listData != null)
                {
                    var data = (SymbolResultInfo)listData.Data;
                    if (data != null && data.SymbolType != SymbolType.None)
                    {
                        SoundManager.Instance.PlayStaticSe(SEType.Decide);
                        var eventData = new TacticsViewEvent(CommandType.SelectSymbol)
                        {
                            template = data
                        };
                        _commandData(eventData);
                    }
                }
            } else
            {
                var getItemInfo = tacticsSymbolList.GetItemInfo();
                if (getItemInfo != null && (getItemInfo.IsSkill() || getItemInfo.IsAttributeSkill()))
                {
                    var eventData = new TacticsViewEvent(CommandType.PopupSkillInfo)
                    {
                        template = getItemInfo
                    };
                    _commandData(eventData);
                }
            }
        }

        private void OnClickEnemyInfo(int selectIndex = -1)
        {
            var listData = tacticsSymbolList.ListData;
            if (listData != null)
            {
                var data = (SymbolResultInfo)listData.Data;
                if (data != null && data.SymbolType != SymbolType.None)
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
                    var eventData = new TacticsViewEvent(CommandType.CallEnemyInfo)
                    {
                        template = data
                    };
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

        public void SetMultiverse(bool enable)
        {
            leftButton?.gameObject?.SetActive(enable);
            rightButton?.gameObject?.SetActive(enable);
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

        public void SetSaveScore(int saveScore)
        {
            saveScoreText?.SetText(DataSystem.GetReplaceDecimalText(saveScore));
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
            alcanaSelectList.SetInputHandler(InputKeyType.Decide,() => 
            {
                if (AlcanaSelectSkillInfo() != null)
                {
                    var eventData = new TacticsViewEvent(CommandType.SelectAlcanaList)
                    {
                        template = AlcanaSelectSkillInfo()
                    };
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
                var data = (SkillInfo)listData.Data;
                if (data != null && data.Enable)
                {
                    return data;
                }
            }
            return null;
        }

        public void SetNuminous(int numinous)
        {
            trainView.SetNuminous(numinous);
        }

        public void UpdateInputKeyActive(TacticsViewEvent viewEvent,TacticsCommandType currentTacticsCommandType)
        {
            switch (viewEvent.commandType)
            {
                case CommandType.SelectTacticsCommand:
                    var tacticsCommandType = (TacticsCommandType)viewEvent.template;
                    switch (tacticsCommandType)
                    {
                        case TacticsCommandType.Paradigm:
                            symbolRecordList.Activate();
                            tacticsCommandList.Deactivate();
                            break;
                        case TacticsCommandType.Train:
                        case TacticsCommandType.Alchemy:
                            tacticsCommandList.Deactivate();
                            break;
                        case TacticsCommandType.Status:
                            break;
                    }
                    break;
                case CommandType.SelectRecord:
                    symbolRecordList.Deactivate();
                    tacticsSymbolList.Activate();
                    break;
                case CommandType.CancelRecordList:
                    symbolRecordList.Activate();
                    tacticsSymbolList.Deactivate();
                    break;
                case CommandType.CancelSymbolRecord:
                    tacticsCommandList.Activate();
                    symbolRecordList.Deactivate();
                    tacticsSymbolList.Deactivate();
                    break;
            }
        }
        
        public void UpdateChildInputKeyActive(TrainViewEvent viewEvent,TacticsCommandType tacticsCommandType)
        {
            switch (viewEvent.commandType)
            {
                case Train.CommandType.DecideTacticsCommand:
                if (tacticsCommandType != TacticsCommandType.Paradigm)
                {
                    tacticsCommandList.Activate();
                }
                break;
            }
        }

        public void InputHandler(InputKeyType keyType, bool pressed)
        {
            if (keyType == InputKeyType.Option1)
            {
                CallSideMenu();
            }
        }
    }
}