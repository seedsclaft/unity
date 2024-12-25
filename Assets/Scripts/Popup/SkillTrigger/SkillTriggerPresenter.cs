using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using SkillTrigger;

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
            _view.SetTriggerCategoryList(MakeListData(_model.SkillTriggerCategoryList()));
            _view.OpenAnimation();
            CheckTutorialState();
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
                case CommandType.CallSkillSelect:
                    CommandCallSkillSelect();
                    break;
                case CommandType.CallTrigger1Select:
                    CommandCallTrigger1Select();
                    break;
                case CommandType.CallTrigger2Select:
                    CommandCallTrigger2Select();
                    break;
                case CommandType.CallTriggerUp:
                    CommandCallTriggerUp();
                    break;
                case CommandType.CallTriggerDown:
                    CommandCallTriggerDown();
                    break;
                case CommandType.DecideSkillSelect:
                    CommandDecideSkillSelect((SkillInfo)viewEvent.template);
                    break;
                case CommandType.DecideTrigger1Select:
                    CommandDecideTrigger1Select((SkillTriggerData)viewEvent.template);
                    break;
                case CommandType.DecideTrigger2Select:
                    CommandDecideTrigger2Select((SkillTriggerData)viewEvent.template);
                    break;
                case CommandType.DecideCategory1Select:
                    CommandDecideCategory1Select();
                    break;
                case CommandType.DecideCategory2Select:
                    CommandDecideCategory2Select();
                    break;
                case CommandType.CancelSelect:
                    CommandCancelSelect();
                    break;
                case CommandType.CancelCategory:
                    CommandCancelCategory();
                    break;
                case CommandType.Recommend:
                    CommandRecommend();
                    break;
            }
        }

        private void CheckTutorialState(CommandType commandType = CommandType.None)
        {
            Func<TutorialData,bool> enable = (tutorialData) => 
            {
                var checkFlag = true;
                if (tutorialData.Param1 == 1200)
                {
                    // Activeの魔法を初めて入手するかステージ3の最初
                    checkFlag = _model.StageMembers().Find(a => a.LearnSkillIds().FindAll(b => DataSystem.FindSkill(b).SkillType == SkillType.Active).Count > 0) != null || _model.CurrentStage.Id == 3;
                }
                return checkFlag;
            };
            Func<TutorialData,bool> checkEnd = (tutorialData) => 
            {
                return true;
            };
            var tutorialViewInfo = new TutorialViewInfo
            {
                SceneType = (int)PopupType.SkillTrigger + 100,
                CheckEndMethod = checkEnd,
                CheckMethod = enable,
                EndEvent = () => 
                {
                    _busy = false;
                    CheckTutorialState(commandType);
                }
            };
            _view.CommandCheckTutorialState(tutorialViewInfo);
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
                _view.SetTrigger1List(MakeListData(list));
            }
        }

        private void CommandDecideCategory2Select()
        {
            var index = _view.Trigger2CategoryIndex;
            var skillListIndex = _view.SkillTriggerIndex;
            if (index > -1 && skillListIndex > -1)
            {
                var list = _model.SkillTriggerDataList(skillListIndex,index + 1);
                _view.SetTrigger2List(MakeListData(list));
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

        private void CommandRecommend()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.RecommendActiveSkill();
            CommandRefresh();
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