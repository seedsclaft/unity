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
                CommandDecideTrigger1Select((TriggerType)viewEvent.template);
                break;
                case SkillTrigger.CommandType.DecideTrigger2Select:
                CommandDecideTrigger2Select((TriggerType)viewEvent.template);
                break;
                case SkillTrigger.CommandType.CancelSelect:
                CommandCancelSelect();
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

        private void CommandDecideTrigger1Select(TriggerType triggerType)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var index = _view.SkillTriggerIndex;
            _model.SetSkillTrigger1(index,triggerType);
            _view.HideSelectList();
            CommandRefresh();
        }
        
        private void CommandDecideTrigger2Select(TriggerType triggerType)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var index = _view.SkillTriggerIndex;
            _model.SetSkillTrigger2(index,triggerType);
            _view.HideSelectList();
            CommandRefresh();
        }

        private void CommandCancelSelect()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _view.HideSelectList();
        }

        private void CommandCallSkillSelect()
        {
            var skillInfos = _model.SkillTriggerSkillList();
            _view.SetSkillList(skillInfos);
        }

        private void CommandCallTrigger1Select()
        {
            var skillInfos = _model.SkillTriggerDataList();
            _view.SetTrigger1List(skillInfos);
            
        }
        private void CommandCallTrigger2Select()
        {
            var skillInfos = _model.SkillTriggerDataList();
            _view.SetTrigger2List(skillInfos);
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