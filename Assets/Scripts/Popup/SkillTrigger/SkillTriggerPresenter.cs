using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ryneus
{
    public class SkillTriggerPresenter : BasePresenter
    {
        SkillTriggerModel _model = null;
        SkillTriggerView _view = null;

        private bool _busy = true;
        public SkillTriggerPresenter(SkillTriggerView view)
        {
            _view = view;
            _model = new SkillTriggerModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
            _busy = false;
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetSkillTrigger(_model.SkillTrigger(_view.SkillTriggerViewInfo.ActorId,0));
            _view.SetTriggerCategoryList(GetListData(_model.SkillTriggerCategoryList()));
            _view.OpenAnimation();
        }

        private void UpdateCommand(SkillTriggerViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            //LogOutput.Log(viewEvent.commandType);
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
                case SkillTrigger.CommandType.CallTriggerUp:
                CommandCallTriggerUp();
                break;
                case SkillTrigger.CommandType.CallTriggerDown:
                CommandCallTriggerDown();
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
            _model.SetSkillTrigger(index,1,triggerType);
            _view.HideSelectList();
            _view.HideSelectCategoryList();
            CommandRefresh();
        }
        
        private void CommandDecideTrigger2Select(SkillTriggerData triggerType)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var index = _view.SkillTriggerIndex;
            _model.SetSkillTrigger(index,2,triggerType);
            _view.HideSelectList();
            _view.HideSelectCategoryList();
            CommandRefresh();
        }

        private void CommandDecideCategory1Select()
        {
            var index = _view.Trigger1CategoryIndex;
            var skillListIndex = _view.SkillTriggerIndex;
            if (index > -1 && skillListIndex > -1)
            {
                var list = _model.SkillTriggerDataList(skillListIndex,index + 1);
                _view.SetTrigger1List(GetListData(list));
            }
        }

        private void CommandDecideCategory2Select()
        {
            var index = _view.Trigger2CategoryIndex;
            var skillListIndex = _view.SkillTriggerIndex;
            if (index > -1 && skillListIndex > -1)
            {
                var list = _model.SkillTriggerDataList(skillListIndex,index + 1);
                _view.SetTrigger2List(GetListData(list));
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
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var skillInfos = _model.SkillTriggerSkillList();
            _view.SetSkillList(skillInfos);
        }

        private void CommandCallTrigger1Select()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var index = _view.SkillTriggerIndex;
            var categoryIndex = _model.SelectCategoryIndex(index,0);
            _view.ShowTrigger1Category(categoryIndex);
        }

        private void CommandCallTrigger2Select()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var index = _view.SkillTriggerIndex;
            var categoryIndex = _model.SelectCategoryIndex(index,1);
            _view.ShowTrigger2Category(categoryIndex);
        }

        private void CommandCallTriggerUp()
        {
            var index = _view.SkillTriggerIndex;
            _model.SetTriggerIndexUp(index);
            CommandRefresh();
        }

        private void CommandCallTriggerDown()
        {
            var index = _view.SkillTriggerIndex;
            _model.SetTriggerIndexDown(index);
            CommandRefresh();
        }

        private void CommandRefresh()
        {
            var selectIndex = _view.SkillTriggerIndex;
            _view.RefreshSkillTrigger(_model.SkillTrigger(_view.SkillTriggerViewInfo.ActorId,selectIndex));
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