using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SkillTrigger;

namespace Ryneus
{
    public class SkillTriggerView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private SkillTriggerList skillTriggerList = null;
        [SerializeField] private MagicList skillList = null;
        [SerializeField] private SkillTriggerDataList trigger1List = null;
        [SerializeField] private SkillTriggerDataList trigger2List = null;
        [SerializeField] private BaseList triggerCategory1List = null;
        [SerializeField] private BaseList triggerCategory2List = null;
        [SerializeField] private OnOffButton recommendButton = null;
        [SerializeField] private Button listBlock = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        private new Action<SkillTriggerViewEvent> _commandData = null;
        public int SkillTriggerIndex => skillTriggerList.Index;
        private SkillTriggerViewInfo _skillTriggerViewInfo;


        public int Trigger1CategoryIndex => triggerCategory1List.Index;
        public int Trigger2CategoryIndex => triggerCategory2List.Index;
        
        public SkillTriggerViewInfo SkillTriggerViewInfo => _skillTriggerViewInfo;
        public override void Initialize() 
        {
            base.Initialize();
            skillList.Initialize();
            skillList.SetInputHandler(InputKeyType.Decide,() => OnClickSkillSelect());
            skillList.SetInputHandler(InputKeyType.Cancel,() => CancelSelect());
            trigger1List.Initialize();
            trigger1List.SetInputHandler(InputKeyType.Decide,() => OnClickTrigger1Select());
            trigger1List.SetInputHandler(InputKeyType.Cancel,() => CancelSelect());

            trigger2List.Initialize();
            trigger2List.SetInputHandler(InputKeyType.Decide,() => OnClickTrigger2Select());
            trigger2List.SetInputHandler(InputKeyType.Cancel,() => CancelSelect());
            HideSelectList();
            triggerCategory1List.Initialize();
            triggerCategory1List.SetSelectedHandler(() => OnClickTriggerCategory1Select());
            triggerCategory1List.SetInputHandler(InputKeyType.Cancel,() => CancelCategory());
            
            triggerCategory2List.Initialize();
            triggerCategory2List.SetSelectedHandler(() => OnClickTriggerCategory2Select());
            triggerCategory2List.SetInputHandler(InputKeyType.Cancel,() => CancelCategory());
            HideSelectCategoryList();
            
            skillTriggerList.Initialize();
            skillTriggerList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            skillTriggerList.SetInputCallHandler();
            SetInputHandler(skillTriggerList.GetComponent<IInputHandlerEvent>());
            listBlock.onClick.AddListener(() => 
            {
                CancelSelect();
            });
            recommendButton.OnClickAddListener(() => 
            {
                var eventData = new SkillTriggerViewEvent(CommandType.Recommend);
                _commandData(eventData);
            });
            SetBaseAnimation(popupAnimation);
            new SkillTriggerPresenter(this);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform,null);
        }

        public void SetSkillTriggerViewInfo(SkillTriggerViewInfo skillTriggerViewInfo)
        {
            _skillTriggerViewInfo = skillTriggerViewInfo;
        }
        
        public void SetEvent(Action<SkillTriggerViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetSkillTrigger(List<ListData> skillTriggerLists)
        {
            skillTriggerList.SetData(skillTriggerLists,true,() => {skillTriggerList.UpdateListItem(skillTriggerList.Index);});
            skillTriggerList.SetInputListHandler(
                () => CallSkillEvent(),
                () => CallTrigger1Event(),
                () => CallTrigger2Event(),
                () => CallTriggerUpEvent(),
                () => CallTriggerDownEvent()
            );
            skillTriggerList.Activate();
        }

        public void RefreshSkillTrigger(List<ListData> skillTriggerLists)
        {
            skillTriggerList.RefreshListData(skillTriggerLists);
        }

        public void HideSelectList()
        {
            skillList.Hide();
            trigger1List.gameObject.SetActive(false);
            trigger2List.gameObject.SetActive(false);
            listBlock.gameObject.SetActive(false);
        }

        public void ShowTrigger1Category(int selectIndex)
        {
            triggerCategory1List.gameObject.SetActive(true);
            triggerCategory1List.UpdateSelectIndex(-1);
            // 選択番号を変化させてリスト表示する
            triggerCategory1List.UpdateSelectIndex(selectIndex);
        }

        public void ShowTrigger2Category(int selectIndex)
        {
            triggerCategory2List.gameObject.SetActive(true);
            triggerCategory2List.UpdateSelectIndex(-1);
            // 選択番号を変化させてリスト表示する
            triggerCategory2List.UpdateSelectIndex(selectIndex);
        }

        public void HideSelectCategoryList()
        {
            triggerCategory1List.gameObject.SetActive(false);
            triggerCategory2List.gameObject.SetActive(false);
            listBlock.gameObject.SetActive(false);
            
        }

        public void SetSkillList(List<ListData> skillInfos)
        {
            skillList.Show();
            listBlock.gameObject.SetActive(true);
            skillList.SetData(skillInfos);
        }

        public void SetTriggerCategoryList(List<ListData> triggerDates)
        {
            triggerCategory1List.SetData(triggerDates);
            triggerCategory2List.SetData(triggerDates);
        }

        public void SetTrigger1List(List<ListData> triggerDates)
        {
            trigger1List.gameObject.SetActive(true);
            listBlock.gameObject.SetActive(true);
            trigger1List.SetData(triggerDates);
        }

        public void SetTrigger2List(List<ListData> triggerDates)
        {
            trigger2List.gameObject.SetActive(true);
            listBlock.gameObject.SetActive(true);
            trigger2List.SetData(triggerDates);
        }

        private void OnClickSkillSelect()
        {
            var data = skillList.ListItemData<SkillInfo>();
            if (data != null)
            {
                var eventData = new SkillTriggerViewEvent(CommandType.DecideSkillSelect)
                {
                    template = data
                };
                _commandData(eventData);
            }
        }

        private void OnClickTrigger1Select()
        {
            var data = trigger1List.ListItemData<SkillTriggerData>();
            if (data != null)
            {
                var eventData = new SkillTriggerViewEvent(CommandType.DecideTrigger1Select)
                {
                    template = data
                };
                _commandData(eventData);
            }
        }

        private void OnClickTrigger2Select()
        {
            var data = trigger2List.ListItemData<SkillTriggerData>();
            if (data != null)
            {
                var eventData = new SkillTriggerViewEvent(CommandType.DecideTrigger2Select)
                {
                    template = data
                };
                _commandData(eventData);
            }
        }

        private void OnClickTriggerCategory1Select()
        {
            var eventData = new SkillTriggerViewEvent(CommandType.DecideCategory1Select);
            _commandData(eventData);
        }

        private void OnClickTriggerCategory2Select()
        {
            var eventData = new SkillTriggerViewEvent(CommandType.DecideCategory2Select);
            _commandData(eventData);
        }

        private void CancelSelect()
        {
            var eventData = new SkillTriggerViewEvent(CommandType.CancelSelect);
            _commandData(eventData);
        }

        private void CancelCategory()
        {
            var eventData = new SkillTriggerViewEvent(CommandType.CancelCategory);
            _commandData(eventData);
        }

        private void CallSkillEvent()
        {
            var data = skillTriggerList.ListItemData<SkillTriggerInfo>();
            if (data != null)
            {
                var eventData = new SkillTriggerViewEvent(CommandType.CallSkillSelect)
                {
                    template = data
                };
                _commandData(eventData);
            }
        }

        private void CallTrigger1Event()
        {   
            var data = skillTriggerList.ListItemData<SkillTriggerInfo>();
            if (data != null)
            {
                var eventData = new SkillTriggerViewEvent(CommandType.CallTrigger1Select)
                {
                    template = data
                };
                _commandData(eventData);
            }
        }

        private void CallTrigger2Event()
        {
            var data = skillTriggerList.ListItemData<SkillTriggerInfo>();
            if (data != null)
            {
                var eventData = new SkillTriggerViewEvent(CommandType.CallTrigger2Select)
                {
                    template = data
                };
                _commandData(eventData);
            }
        }

        private void CallTriggerUpEvent()
        {
            var data = skillTriggerList.ListItemData<SkillTriggerInfo>();
            if (data != null)
            {
                var eventData = new SkillTriggerViewEvent(CommandType.CallTriggerUp)
                {
                    template = data
                };
                _commandData(eventData);
            }
        }

        private void CallTriggerDownEvent()
        {
            var data = skillTriggerList.ListItemData<SkillTriggerInfo>();
            if (data != null)
            {
                var eventData = new SkillTriggerViewEvent(CommandType.CallTriggerDown)
                {
                    template = data
                };
                _commandData(eventData);
            }
        }

        public void InputHandler(InputKeyType keyType,bool pressed)
        {
            switch (keyType)
            {
                case InputKeyType.Right:
                    break;
                case InputKeyType.Left:
                    break;
            }
        }
    }

}

namespace SkillTrigger
{
    public enum CommandType
    {
        CallSkillSelect,
        CallTrigger1Select,
        CallTrigger2Select,
        CallTriggerUp,
        CallTriggerDown,
        DecideSkillSelect,
        DecideTrigger1Select,
        DecideTrigger2Select,
        DecideCategory1Select,
        DecideCategory2Select,
        CancelSelect,
        CancelCategory,
        Recommend,
        None = 0,
    }
}

public class SkillTriggerViewEvent
{
    public CommandType commandType;
    public object template;

    public SkillTriggerViewEvent(CommandType type)
    {
        commandType = type;
    }
}