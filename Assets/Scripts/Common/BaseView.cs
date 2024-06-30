using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utage;

namespace Ryneus
{
    abstract public class BaseView : MonoBehaviour
    {
        private bool _testMode = false;
        public bool TestMode => _testMode;
        private bool _testBattleMode = false;
        public bool TestBattleMode => _testBattleMode;
        private InputSystem _inputSystem;
        private InputSystemModel _inputSystemModel = null;
        private bool _busy = false;
        public bool Busy => _busy;
        public System.Action<ViewEvent> _commandData = null;
        [SerializeField] private Button _backCommand = null;
        [SerializeField] private SpriteRenderer _backGround = null;
        private System.Action _backEvent = null;
        public System.Action BackEvent => _backEvent;
        [SerializeField] private GameObject uiRoot = null;
        [SerializeField] private Button sideMenuButton = null;
        public Button SideMenuButton => sideMenuButton;
        private int _wait = 0;
        public System.Action _waitEndEvent = null;

        private HelpWindow _helpWindow = null;
        public HelpWindow HelpWindow => _helpWindow;
        public void SetHelpInputInfo(string key)
        {
            _helpWindow?.SetInputInfo(key);
        }

        public void SetHelpText(string text)
        {
            _helpWindow?.SetHelpText(text);
        }

        public void SetBackGround(string fileName)
        {
            _backGround.sprite = ResourceSystem.LoadBackGround(fileName);
        }

        public void SetInputFrame(int frame)
        {
            _inputSystemModel.SetInputFrame(frame);
        }

        public virtual void Initialize()
        {
            _inputSystemModel = new InputSystemModel();
            InitializeInput();
            SetInputHandler(gameObject);
        }

        public void InitializeInput()
        {    
            _inputSystem = new InputSystem();
        }

        public void SetHelpWindow(HelpWindow helpWindow)
        {
            _helpWindow = helpWindow;
        }

        public void SetInputHandler(IInputHandlerEvent handler)
        {
            _inputSystemModel.AddInputHandler(handler);
        }

        public void SetInputHandler(GameObject gameObject)
        {
            var handler = gameObject.GetComponent<IInputHandlerEvent>();
            if (handler != null)
            {
                SetInputHandler(handler);
            }
        }

        public void SetBusy(bool isBusy)
        {
            _busy = isBusy;
        }

        public void Update()
        {
            if (_inputSystem != null && _busy == false)
            {
                _inputSystemModel.UpdateInputKeyType(_inputSystem.Update());
            }
            UpdateWait();
        }

        private void UpdateWait()
        {
            if (_wait <= 0) return;
            _busy = true;
            _wait--;
            if (_wait <= 0)
            {
                _busy = false;
                _waitEndEvent?.Invoke();
            }
        }

        public void CommandOpenSideMenu()
        {
            _helpWindow.SetInputInfo("SIDEMENU");
            _helpWindow.SetHelpText(DataSystem.GetHelp(701));
        }

        public void SetEvent(System.Action<ViewEvent> commandData)
        {
            _commandData = commandData;
        }

        private void CallSceneChangeCommand(ViewEvent eventData)
        {
            _commandData(eventData);
        }

        public void CommandGameSystem(Base.CommandType commandType)
        {
            var eventData = new ViewEvent(commandType);
            CallSceneChangeCommand(eventData);
        }

        public void CommandSceneChange(Scene scene,object sceneParam = null,SceneChangeType sceneChangeType = SceneChangeType.Push)
        {
            var eventData = new ViewEvent(Base.CommandType.SceneChange);
            var sceneInfo = new SceneInfo(){ToScene = scene,SceneChangeType = sceneChangeType,SceneParam = sceneParam};
            eventData.template = sceneInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandPopSceneChange(object sceneParam = null)
        {
            var eventData = new ViewEvent(Base.CommandType.SceneChange);
            var sceneInfo = new SceneInfo(){SceneChangeType = SceneChangeType.Pop,SceneParam = sceneParam};
            eventData.template = sceneInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandGotoSceneChange(Scene scene,object sceneParam = null)
        {
            var eventData = new ViewEvent(Base.CommandType.SceneChange);
            var sceneInfo = new SceneInfo(){ToScene = scene,SceneChangeType = SceneChangeType.Goto,SceneParam = sceneParam};
            eventData.template = sceneInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallConfirm(ConfirmInfo popupInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallConfirmView)
            {
                template = popupInfo
            };
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallSkillDetail(ConfirmInfo popupInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallSkillDetailView)
            {
                template = popupInfo
            };
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallCaution(CautionInfo popupInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallCautionView)
            {
                template = popupInfo
            };
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallPopup(PopupInfo popupInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallPopupView)
            {
                template = popupInfo
            };
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallOption(System.Action endEvent)
        {
            var eventData = new ViewEvent(Base.CommandType.CallOptionView)
            {
                template = endEvent
            };
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallSideMenu(SideMenuViewInfo sideMenuViewInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallSideMenu);
            eventData.template = sideMenuViewInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallRanking(RankingViewInfo rankingViewInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallRankingView);
            eventData.template = rankingViewInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallCharacterList(CharacterListInfo characterListInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallCharacterListView);
            eventData.template = characterListInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandHelpList(List<ListData> helpTextList)
        {
            var eventData = new ViewEvent(Base.CommandType.CallHelpView);
            eventData.template = helpTextList;
            CallSceneChangeCommand(eventData);
        }

        public void CommandSlotSave(SlotSaveViewInfo slotSaveViewInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallSlotSaveView)
            {
                template = slotSaveViewInfo
            };
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallStatus(StatusViewInfo statusViewInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallStatusView)
            {
                template = statusViewInfo
            };
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallEnemyInfo(StatusViewInfo statusViewInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallEnemyInfoView)
            {
                template = statusViewInfo
            };
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallSkillTrigger(SkillTriggerViewInfo skillTriggerViewInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallSkillTriggerView)
            {
                template = skillTriggerViewInfo
            };
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallSkillLog(SkillLogViewInfo skillLogViewInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallSkillLogView)
            {
                template = skillLogViewInfo
            };
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallAdv(AdvCallInfo advCallInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallAdvScene)
            {
                template = advCallInfo
            };
            CallSceneChangeCommand(eventData);
        }

        public void CommandDecidePlayerName(string nameText)
        {
            var eventData = new ViewEvent(Base.CommandType.DecidePlayerName)
            {
                template = nameText
            };
            CallSceneChangeCommand(eventData);
        }

        public void CommandSetRouteSelect()
        {
            var eventData = new ViewEvent(Base.CommandType.SetRouteSelect);
            CallSceneChangeCommand(eventData);
        }

        public void CommandChangeViewToTransition(System.Action<string> endEvent)
        {
            var eventData = new ViewEvent(Base.CommandType.ChangeViewToTransition)
            {
                template = endEvent
            };
            CallSceneChangeCommand(eventData);
        }

        public void CommandStartTransition(System.Action endEvent)
        {
            var eventData = new ViewEvent(Base.CommandType.StartTransition)
            {
                template = endEvent
            };
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallTutorialFocus(StageTutorialData stageTutorialDate)
        {
            var eventData = new ViewEvent(Base.CommandType.CallTutorialFocus)
            {
                template = stageTutorialDate
            };
            CallSceneChangeCommand(eventData);
        }

        public void CommandCloseTutorialFocus()
        {
            var eventData = new ViewEvent(Base.CommandType.CloseTutorialFocus);
            CallSceneChangeCommand(eventData);
        }

        public void CommandSceneShowUI()
        {
            var eventData = new ViewEvent(Base.CommandType.SceneShowUI);
            CallSceneChangeCommand(eventData);
        }

        public void CommandSceneHideUI()
        {
            var eventData = new ViewEvent(Base.CommandType.SceneHideUI);
            CallSceneChangeCommand(eventData);
        }

        public void SetBackCommand(System.Action callEvent)
        {
            if (_backCommand != null)
            {
                _backCommand.onClick.AddListener(() => {
                            
                    if (!_backCommand.gameObject.activeSelf) return;
                    callEvent();
                });
            }
            _backEvent = callEvent;
        }

        public void SetBackEvent(System.Action backEvent)
        {
            SetBackCommand(() => 
            {
                backEvent?.Invoke();
            });
            ChangeBackCommandActive(true);
        }
        
        public void ChangeBackCommandActive(bool IsActive)
        {
            _backCommand?.gameObject.SetActive(IsActive);
        }

        public void ChangeUIActive(bool IsActive)
        {
            uiRoot.SetActive(IsActive);
        }

        public void SetTestMode(bool isTest)
        {
            _testMode = isTest;
        }

        public void SetBattleTestMode(bool isTest)
        {
            _testBattleMode = isTest;
        }

        public void MouseCancelHandler()
        {

        }

        public void WaitFrame(int frame,System.Action waitEndEvent)
        {
            _wait = frame;
            _waitEndEvent = waitEndEvent;
        }

        private void OnDestroy() 
        {
            var listViews = GetComponentsInChildren<ListWindow>();
            for (int i = listViews.Length-1;i >= 0;i--)
            {
                listViews[i].Release();
            }
        }

    }

    namespace Base
    {
        public enum CommandType
        {
            None = 0,
            SceneChange,
            CallConfirmView,
            CallSkillDetailView,
            CallCautionView,
            CallPopupView,
            ClosePopup,
            CloseConfirm,
            CallOptionView,
            CallSideMenu,
            CallRankingView,
            CallCharacterListView,
            CallHelpView,
            CallSlotSaveView,
            CallStatusView,
            CloseStatus,
            CallAdvScene,
            CallEnemyInfoView,
            CallSkillTriggerView,
            CallSkillLogView,
            DecidePlayerName,
            CallLoading,
            CloseLoading,
            SetRouteSelect,
            ChangeViewToTransition,
            StartTransition,
            CallTutorialFocus,
            CloseTutorialFocus,
            SceneShowUI,
            SceneHideUI
        }
    }
}