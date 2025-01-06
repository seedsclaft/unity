using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Ryneus
{
    using Tactics;
    public class TacticsView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private BaseList tacticsCommandList = null;
        [SerializeField] private StageInfoComponent stageInfoComponent = null;
        [SerializeField] private AlcanaInfoComponent alcanaInfoComponent = null;
        [SerializeField] private SymbolList symbolInfoList = null;
        public SymbolInfo SelectSymbolInfo => symbolInfoList.SelectSymbolInfo();
        [SerializeField] private MagicList alcanaSelectList = null;
        [SerializeField] private TextMeshProUGUI saveScoreText = null;
        private new System.Action<ViewEvent> _commandData = null;
        [SerializeField] private TacticsAlcana tacticsAlcana = null;
        [SerializeField] private Button alcanaButton = null;
        [SerializeField] private Button stageHelpButton = null;
        [SerializeField] private TextMeshProUGUI numinousText = null;
        [SerializeField] private _2dxFX_NoiseAnimated _2DxFX_NoiseAnimated = null;
        [SerializeField] private TextMeshProUGUI pastText = null;

        private bool _viewBusy = false;
        public void SetViewBusy(bool isBusy)
        {
            _viewBusy = isBusy;
        }        
        public void CallEvent(CommandType tacticsCommandType,object sendData = null)
        {
            var commandType = new ViewCommandType(ViewCommandSceneType.Tactics,tacticsCommandType);
            var eventData = new ViewEvent(commandType)
            {
                template = sendData
            };
            _commandData(eventData);
        }
        
        public override void Initialize()
        {
            base.Initialize();

            InitializeCommandList();
            tacticsAlcana.gameObject.SetActive(false);
            alcanaButton.onClick.AddListener(() => CallAlcanaCheck());

            SideMenuButton.OnClickAddListener(() => 
            {
                CallSideMenu();
            });
            stageHelpButton.onClick.AddListener(() => 
            {
                CallEvent(CommandType.StageHelp);
            });
            InitializeSymbolInfoList();

            alcanaSelectList.Initialize();
            HideSymbolRecord();
            alcanaSelectList.Hide();
            var presenter = new TacticsPresenter(this);
            presenter.CommandReturnStrategy();
        }

        private void InitializeCommandList()
        {
            tacticsCommandList.Initialize();
            tacticsCommandList.SetInputHandler(InputKeyType.Decide,() => CallSymbolList());
            tacticsCommandList.SetInputHandler(InputKeyType.Option1,() => CallStatus());
            tacticsCommandList.SetSelectedHandler(() => UpdateHelpWindow());
            SetInputHandler(tacticsCommandList.gameObject);
            AddViewActives(tacticsCommandList);
        }

        private void CallSymbolList()
        {
            var listData = tacticsCommandList.ListData;
            if (listData != null && listData.Enable)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                CallEvent(CommandType.CallSymbolList);
            }
        }

        private void CallStatus()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CallEvent(CommandType.CallStatus);
        }

        private void InitializeSymbolInfoList()
        {
            symbolInfoList.Initialize();
            SetInputHandler(symbolInfoList.gameObject);
            symbolInfoList.SetInputHandler(InputKeyType.Decide,OnClickSymbol);
            symbolInfoList.SetInputHandler(InputKeyType.Cancel,OnCancelSymbol);
            //symbolInfoList.SetSelectedHandler(OnSelectListSymbolList);
            //symbolInfoList.SetInputHandler(InputKeyType.Cancel,OnCancelActor);
            AddViewActives(symbolInfoList);
            symbolInfoList.gameObject.SetActive(false);
        }

        public void SetSymbolList(List<ListData> symbolList,int seekIndex,int seek)
        {
            symbolInfoList.SetSeekIndex(seekIndex);
            symbolInfoList.SetData(symbolList,true,() => 
            {
                var selectIndex = symbolInfoList.DataCount - seek;
                if (selectIndex < 0)
                {
                    selectIndex = 0;
                }
                symbolInfoList.UpdateSelectIndex(selectIndex);
                symbolInfoList.UpdateScrollRect(selectIndex + 2);
            });
            SetActivate(symbolInfoList);
        }

        private void OnClickSymbol()
        {
            if (symbolInfoList.ScrollRect.enabled == false) return;
            var data = symbolInfoList.SelectSymbolInfo();
            if (data != null)
            {
                CallEvent(CommandType.OnClickSymbol,data);
            }
        }

        private void OnCancelSymbol()
        {
            symbolInfoList.gameObject.SetActive(false);
            CallEvent(CommandType.OnCancelSymbol);
        }

        public void UpdatePartyInfo(PartyInfo partyInfo)
        {
            symbolInfoList.UpdatePartyInfo(partyInfo);
        }

        public void StartAnimation()
        {
        }

        private void CallSideMenu()
        {
            CallEvent(CommandType.SelectSideMenu);
        }

        public void SetTacticsCommand(List<ListData> menuCommands)
        {
            tacticsCommandList.SetData(menuCommands);
            UpdateHelpWindow();
            SetActivate(tacticsCommandList);
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

        private void OnClickBack()
        {
            CallEvent(CommandType.Back);
        }

        public void SetHelpWindow()
        {
        }

        public new void SetEvent(System.Action<ViewEvent> commandData)
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

        public void ShowSymbolRecord()
        {
            symbolInfoList.gameObject.SetActive(true);
        }

        public void HideSymbolRecord()
        {
            symbolInfoList.gameObject.SetActive(false);
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
                        CallEvent(CommandType.SelectSymbol)
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
                    CallEvent(CommandType.PopupSkillInfo)
                    {
                        template = getItemInfos
                    };
                    _commandData(eventData);
                } else
                {
                    var getItemInfo = tacticsSymbolList.GetItemInfo();
                    if (getItemInfo != null && (getItemInfo.IsSkill() || getItemInfo.IsAttributeSkill()))
                    {
                        CallEvent(CommandType.PopupSkillInfo)
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
                            CallEvent(CommandType.CallAddActorInfo)
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
            CallEvent(CommandType.Parallel);
        }


        public void ShowRecordList()
        {
            //symbolInfoList.ScrollRect.enabled = false;
        }

        public void HideRecordList()
        {
            //symbolInfoList.ScrollRect.enabled = true;
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
            CallEvent(CommandType.AlcanaCheck);
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
                    CallEvent(CommandType.SelectAlcanaList,skillInfo);
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
            //numinousText?.SetText(numinous.ToString());
        }

        public void SetPastMode(bool pastMode)
        {
            pastText?.gameObject.SetActive(pastMode);
            _2DxFX_NoiseAnimated.enabled = pastMode;
        }

        public void CommandSelectCharaLayer(int actorId)
        {
        }

        public void ActivateCommandList()
        {
            SetActivate(tacticsCommandList);
        }

        public void EndStatusCursor()
        {
        }

        public void UpdateInputKeyActive(ViewCommandType viewEvent,TacticsCommandType currentTacticsCommandType)
        {
            /*
            switch (viewEvent.TacticsCommandType)
            {
                case CommandType.SelectTacticsCommand:
                    var tacticsCommandType = (TacticsCommandType)viewEvent.TacticsCommandType;
                    switch (tacticsCommandType)
                    {
                        case TacticsCommandType.Paradigm:
                            symbolInfoList.Activate();
                            tacticsCommandList.Deactivate();
                            break;
                        case TacticsCommandType.Train:
                        case TacticsCommandType.Alchemy:
                        case TacticsCommandType.Status:
                            break;
                    }
                    break;
                case CommandType.SelectRecord:
                    symbolInfoList.Deactivate();
                    break;
                case CommandType.CancelRecordList:
                    symbolInfoList.Activate();
                    break;
                case CommandType.CancelSymbolRecord:
                    tacticsCommandList.Activate();
                    symbolInfoList.Deactivate();
                    break;
            }
            */
        }

        public void InputHandler(InputKeyType keyType, bool pressed)
        {
        }

        public void CommandRefresh()
        {
            if (symbolInfoList.gameObject.activeSelf && symbolInfoList.Active)
            {
                SetHelpInputInfo("RECORD_LIST");
            } else
            {
                SetHelpInputInfo("TACTICS");
            }
        }
    }

}