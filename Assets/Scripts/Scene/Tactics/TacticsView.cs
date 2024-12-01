using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tactics;
using TMPro;
using DG.Tweening;

namespace Ryneus
{
    public class TacticsView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private BaseList tacticsCommandList = null;
        [SerializeField] private StageInfoComponent stageInfoComponent = null;
        [SerializeField] private AlcanaInfoComponent alcanaInfoComponent = null;
        [SerializeField] private BaseList symbolRecordList = null;
        [SerializeField] private MagicList alcanaSelectList = null;
        [SerializeField] private TextMeshProUGUI saveScoreText = null;
        private new System.Action<TacticsViewEvent> _commandData = null;
        [SerializeField] private TacticsAlcana tacticsAlcana = null;
        [SerializeField] private Button alcanaButton = null;
        [SerializeField] private Button stageHelpButton = null;
        [SerializeField] private TextMeshProUGUI numinousText = null;
        [SerializeField] private _2dxFX_NoiseAnimated _2DxFX_NoiseAnimated = null;
        [SerializeField] private TextMeshProUGUI pastText = null;

        private bool _initRecordDisplay = false;

        public override void Initialize()
        {
            base.Initialize();


            tacticsCommandList.Initialize();
            tacticsAlcana.gameObject.SetActive(false);
            alcanaButton.onClick.AddListener(() => CallAlcanaCheck());

            SideMenuButton.OnClickAddListener(() => 
            {
                CallSideMenu();
            });
            stageHelpButton.onClick.AddListener(() => 
            {
                var eventData = new TacticsViewEvent(CommandType.StageHelp);
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

            alcanaSelectList.Initialize();
            HideSymbolRecord();
            alcanaSelectList.Hide();
            var presenter = new TacticsPresenter(this);
            presenter.CommandReturnStrategy();
        }

        public void StartAnimation()
        {
        }

        private void CallSideMenu()
        {
            var eventData = new TacticsViewEvent(CommandType.SelectSideMenu);
            _commandData(eventData);
        }

        public void SetTacticsCommand(List<ListData> menuCommands)
        {
            tacticsCommandList.SetData(menuCommands);
            tacticsCommandList.SetInputHandler(InputKeyType.Decide,() => CallSelectTacticsCommand());
            tacticsCommandList.SetInputHandler(InputKeyType.Option1,() => CallSideMenu());
            tacticsCommandList.SetSelectedHandler(() => UpdateHelpWindow());
            SetInputHandler(tacticsCommandList.gameObject);
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
        }

        public void ChangeSymbolBackCommandActive(bool IsActive)
        {
        }

        private void OnClickBack()
        {
            var eventData = new TacticsViewEvent(CommandType.Back);
            _commandData(eventData);
        }

        private void OnClickLeft()
        {
        }

        private void OnClickRight()
        {
        }

        public void SetHelpWindow()
        {
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
        }

        public void SetSymbols(List<ListData> symbolInfos)
        {
            HideRecordList();
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
            /*
            if (symbolRecordList.ScrollRect.enabled == false) return;
            var data = symbolRecordList.ListItemData<List<SymbolResultInfo>>();
            if (data != null)
            {
                if (data[0].SymbolType != SymbolType.None)
                {
                    var eventData = new TacticsViewEvent(CommandType.SelectRecord)
                    {
                        template = data[0]
                    };
                    _commandData(eventData);
                }
            }
            */
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
            /*
            if (tacticsSymbolList.IsSelectSymbol())
            {
                var data = tacticsSymbolList.ListItemData<SymbolResultInfo>();
                if (data != null)
                {
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
                var getItemInfos = tacticsSymbolList.SelectRelicInfos();
                if (getItemInfos != null && getItemInfos.Count > 0)
                {
                    var eventData = new TacticsViewEvent(CommandType.PopupSkillInfo)
                    {
                        template = getItemInfos
                    };
                    _commandData(eventData);
                } else
                {
                    var getItemInfo = tacticsSymbolList.GetItemInfo();
                    if (getItemInfo != null && (getItemInfo.IsSkill() || getItemInfo.IsAttributeSkill()))
                    {
                        var eventData = new TacticsViewEvent(CommandType.PopupSkillInfo)
                        {
                            template = new List<GetItemInfo>(){getItemInfo}
                        };
                        _commandData(eventData);
                    }
                    if (getItemInfo != null && getItemInfo.IsAddActor())
                    {
                        var data = tacticsSymbolList.ListItemData<SymbolResultInfo>();
                        if (data != null)
                        {
                            var eventData = new TacticsViewEvent(CommandType.CallAddActorInfo)
                            {
                                template = data
                            };
                            _commandData(eventData);
                        }
                    }
                }
            }
            */
        }

        private void OnClickEnemyInfo()
        {
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

        public void ShowRecordList()
        {
            symbolRecordList.ScrollRect.enabled = false;
        }

        public void HideRecordList()
        {
            symbolRecordList.ScrollRect.enabled = true;
        }

        public void SetSaveScore(float saveScore)
        {
            saveScoreText?.SetText("+" + saveScore.ToString("F2"));
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
            SetBackEvent(() => OnClickBack());
            alcanaSelectList.SetData(skillInfos);
            alcanaSelectList.SetInputHandler(InputKeyType.Decide,() => 
            {
                var skillInfo = AlcanaSelectSkillInfo();
                if (skillInfo != null && skillInfo.Enable)
                {
                    var eventData = new TacticsViewEvent(CommandType.SelectAlcanaList)
                    {
                        template = skillInfo
                    };
                    _commandData(eventData);
                }
            });
            alcanaSelectList.Show();
        }

        public SkillInfo AlcanaSelectSkillInfo() 
        {
            return alcanaSelectList.ListItemData<SkillInfo>();
        }

        public void SetNuminous(int numinous)
        {
            numinousText?.SetText(numinous.ToString());
        }

        public void SetPastMode(bool pastMode)
        {
            pastText?.gameObject.SetActive(pastMode);
            _2DxFX_NoiseAnimated.enabled = pastMode;
        }

        public void CommandSelectCharaLayer(int actorId)
        {
        }

        public void EndStatus()
        {
        }

        public void EndStatusCursor()
        {
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
                        case TacticsCommandType.Status:
                            break;
                    }
                    break;
                case CommandType.SelectRecord:
                    symbolRecordList.Deactivate();
                    break;
                case CommandType.CancelRecordList:
                    symbolRecordList.Activate();
                    break;
                case CommandType.CancelSymbolRecord:
                    tacticsCommandList.Activate();
                    symbolRecordList.Deactivate();
                    break;
            }
        }

        public void InputHandler(InputKeyType keyType, bool pressed)
        {
        }

        public void CommandRefresh()
        {
            if (symbolRecordList.gameObject.activeSelf && symbolRecordList.Active)
            {
                SetHelpInputInfo("RECORD_LIST");
            } else
            {
                SetHelpInputInfo("TACTICS");
            }
        }
    }
}