using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class SkillTriggerPresenter 
    {
        SkillTriggerModel _model = null;
        SkillTriggerView _view = null;

        private bool _busy = true;
        public SkillTriggerPresenter(SkillTriggerView view)
        {
            _view = view;
            _model = new SkillTriggerModel();

            Initialize();
            _busy = false;
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetSkillTrigger(_model.SkillTrigger(_view.SkillTriggerViewInfo.ActorId));
            _view.SetTriggerCategoryList(_model.SkillTriggerCategoryList());
        }

        private void UpdateCommand(SkillTriggerViewEvent viewEvent)
        {
            if (_busy){
                return;
            }
            switch (viewEvent.commandType)
            {
                case SkillTrigger.CommandType.CallSkillSelect:
                CommandCallSkillSelect();
                break;
                case SkillTrigger.CommandType.CallTrigger1Select:
                CommandCallTrigger1Select();
                break;
                case SkillTrigger.CommandType.CallTrigger2Select:
                CommandCallTrigger2Select();
                break;
                case SkillTrigger.CommandType.DecideSkillSelect:
                CommandDecideSkillSelect((SkillInfo)viewEvent.template);
                break;
                case SkillTrigger.CommandType.DecideTrigger1Select:
                CommandDecideTrigger1Select((SkillTriggerData)viewEvent.template);
                break;
                case SkillTrigger.CommandType.DecideTrigger2Select:
                CommandDecideTrigger2Select((SkillTriggerData)viewEvent.template);
                break;
                case SkillTrigger.CommandType.DecideCategory1Select:
                CommandDecideCategory1Select();
                break;
                case SkillTrigger.CommandType.DecideCategory2Select:
                CommandDecideCategory2Select();
                break;
                case SkillTrigger.CommandType.CancelSelect:
                CommandCancelSelect();
                break;
                case SkillTrigger.CommandType.CancelCategory:
                CommandCancelCategory();
                break;
            }
        }

        private void CommandDecideSkillSelect(SkillInfo skillInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var index = _view.SkillTriggerIndex;
            _model.SetSkillTriggerSkill(index,skillInfo.Id);
            _view.HideSelectList();
            CommandRefresh();
        }

        private void CommandDecideTrigger1Select(SkillTriggerData triggerType)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var index = _view.SkillTriggerIndex;
            _model.SetSkillTrigger1(index,triggerType);
            _view.HideSelectList();
            _view.HideSelectCategoryList();
            CommandRefresh();
        }
        
        private void CommandDecideTrigger2Select(SkillTriggerData triggerType)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var index = _view.SkillTriggerIndex;
            _model.SetSkillTrigger2(index,triggerType);
            _view.HideSelectList();
            _view.HideSelectCategoryList();
            CommandRefresh();
        }

        private void CommandDecideCategory1Select()
        {
            var index = _view.Trigger1CategoryIndex;
            var skillListIndex = _view.SkillTriggerListIndex;
            if (index > -1 && skillListIndex > -1)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var list = _model.SkillTriggerDataList(skillListIndex,index + 1);
                _view.SetTrigger1List(list);
            }
        }

        private void CommandDecideCategory2Select()
        {
            var index = _view.Trigger1CategoryIndex;
            var skillListIndex = _view.SkillTriggerListIndex;
            if (index > -1 && skillListIndex > -1)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var list = _model.SkillTriggerDataList(skillListIndex,index + 1);
                _view.SetTrigger2List(list);
            }
        }

        private void CommandCancelSelect()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _view.HideSelectList();
            _view.HideSelectCategoryList();
        }

        private void CommandCancelCategory()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _view.HideSelectList();
            _view.HideSelectCategoryList();
        }

        private void CommandCallSkillSelect()
        {
            var skillInfos = _model.SkillTriggerSkillList();
            _view.SetSkillList(skillInfos);
        }

        private void CommandCallTrigger1Select()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.ShowTrigger1Category();
            
        }
        private void CommandCallTrigger2Select()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.ShowTrigger2Category();
        }

        private void CommandRefresh()
        {
            _view.SetSkillTrigger(_model.SkillTrigger(_view.SkillTriggerViewInfo.ActorId));
        }
    }
    public class SkillTriggerViewInfo
    {
        private System.Action _endEvent = null;
        public System.Action EndEvent => _endEvent;
        private int _actorId;
        public int ActorId => _actorId;
        public SkillTriggerViewInfo(int actorId,System.Action endEvent)
        {
            _endEvent = endEvent;
            _actorId = actorId;
        }
    }
}