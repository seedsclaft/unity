using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class SkillTriggerView : BaseView
    {
        [SerializeField] private SkillTriggerList skillTriggerList = null;
        [SerializeField] private MagicList skillList = null;
        [SerializeField] private SkillTriggerDataList trigger1List = null;
        [SerializeField] private SkillTriggerDataList trigger2List = null;
        [SerializeField] private BaseList triggerCategory1List = null;
        [SerializeField] private BaseList triggerCategory2List = null;
        [SerializeField] private Button listBlock = null;
        [SerializeField] private SkillInfoComponent skillInfoComponent = null;
        [SerializeField] private TextMeshProUGUI trigger1Help = null;
        [SerializeField] private TextMeshProUGUI trigger2Help = null;
        private new Action<SkillTriggerViewEvent> _commandData = null;
        public int SkillTriggerIndex => skillTriggerList.Index;
        private SkillTriggerViewInfo _skillTriggerViewInfo;

        public int SkillTriggerListIndex => skillTriggerList.Index;

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
            skillTriggerList.SetSelectedHandler(() => SelectSkillTriggerList());
            skillTriggerList.SetInputCallHandler();
            SetInputHandler(skillTriggerList.GetComponent<IInputHandlerEvent>());
            listBlock.onClick.AddListener(() => {
                CancelSelect();
            });
            new SkillTriggerPresenter(this);
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
            skillTriggerList.SetData(skillTriggerLists,false);
            skillTriggerList.SetInputListHandler(
                () => CallSkillEvent(),
                () => CallTrigger1Event(),
                () => CallTrigger2Event(),
                () => CallTriggerUpEvent(),
                () => CallTriggerDownEvent()
            );
            skillTriggerList.Activate();
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

        public void UpdateSkillInfo(int skillId)
        {
            skillInfoComponent?.UpdateSkillData(skillId);
        }

        public void UpdateSkillTriggerHelp(string help1,string help2)
        {
            trigger1Help?.SetText(help1);
            trigger2Help?.SetText(help2);
        }

        private void OnClickSkillSelect()
        {
            var listData = skillList.ListData;
            if (listData != null)
            {
                var data = (SkillInfo)listData.Data;
                var eventData = new SkillTriggerViewEvent(SkillTrigger.CommandType.DecideSkillSelect);
                eventData.template = data;
                _commandData(eventData);
            }
        }

        private void SelectSkillTriggerList()
        {
            var listData = skillTriggerList.ListData;
            if (listData != null)
            {
                var data = (SkillTriggerInfo)listData.Data;
                var eventData = new SkillTriggerViewEvent(SkillTrigger.CommandType.SelectSkillTrigger);
                eventData.template = data;
                _commandData(eventData);
            }
        }

        private void OnClickTrigger1Select()
        {
            var listData = trigger1List.ListData;
            if (listData != null)
            {
                var data = (SkillTriggerData)listData.Data;
                var eventData = new SkillTriggerViewEvent(SkillTrigger.CommandType.DecideTrigger1Select);
                eventData.template = data;
                _commandData(eventData);
            }
        }

        private void OnClickTrigger2Select()
        {
            var listData = trigger2List.ListData;
            if (listData != null)
            {
                var data = (SkillTriggerData)listData.Data;
                var eventData = new SkillTriggerViewEvent(SkillTrigger.CommandType.DecideTrigger2Select);
                eventData.template = data;
                _commandData(eventData);
            }
        }

        private void OnClickTriggerCategory1Select()
        {
            var eventData = new SkillTriggerViewEvent(SkillTrigger.CommandType.DecideCategory1Select);
            _commandData(eventData);
        }

        private void OnClickTriggerCategory2Select()
        {
            var eventData = new SkillTriggerViewEvent(SkillTrigger.CommandType.DecideCategory2Select);
            _commandData(eventData);
        }

        private void CancelSelect()
        {
            var eventData = new SkillTriggerViewEvent(SkillTrigger.CommandType.CancelSelect);
            _commandData(eventData);
        }

        private void CancelCategory()
        {
            var eventData = new SkillTriggerViewEvent(SkillTrigger.CommandType.CancelCategory);
            _commandData(eventData);
        }

        private void CallSkillEvent()
        {
            var listData = skillTriggerList.ListData;
            if (listData != null)
            {
                var data = (SkillTriggerInfo)listData.Data;
                var eventData = new SkillTriggerViewEvent(SkillTrigger.CommandType.CallSkillSelect);
                eventData.template = data;
                _commandData(eventData);
            }
        }

        private void CallTrigger1Event()
        {   
            var listData = skillTriggerList.ListData;
            if (listData != null)
            {
                var data = (SkillTriggerInfo)listData.Data;
                var eventData = new SkillTriggerViewEvent(SkillTrigger.CommandType.CallTrigger1Select);
                eventData.template = data;
                _commandData(eventData);
            }
        }

        private void CallTrigger2Event()
        {
            var listData = skillTriggerList.ListData;
            if (listData != null)
            {
                var data = (SkillTriggerInfo)listData.Data;
                var eventData = new SkillTriggerViewEvent(SkillTrigger.CommandType.CallTrigger2Select);
                eventData.template = data;
                _commandData(eventData);
            }
        }

        private void CallTriggerUpEvent()
        {
            var listData = skillTriggerList.ListData;
            if (listData != null)
            {
                var data = (SkillTriggerInfo)listData.Data;
                var eventData = new SkillTriggerViewEvent(SkillTrigger.CommandType.CallTriggerUp);
                eventData.template = data;
                _commandData(eventData);
            }
        }

        private void CallTriggerDownEvent()
        {
            var listData = skillTriggerList.ListData;
            if (listData != null)
            {
                var data = (SkillTriggerInfo)listData.Data;
                var eventData = new SkillTriggerViewEvent(SkillTrigger.CommandType.CallTriggerDown);
                eventData.template = data;
                _commandData(eventData);
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
        SelectSkillTrigger,
        None = 0,
    }
}

public class SkillTriggerViewEvent
{
    public SkillTrigger.CommandType commandType;
    public object template;

    public SkillTriggerViewEvent(SkillTrigger.CommandType type)
    {
        commandType = type;
    }
}