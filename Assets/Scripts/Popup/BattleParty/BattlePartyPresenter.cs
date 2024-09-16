using System.Collections;
using System.Collections.Generic;
using BattleParty;

namespace Ryneus
{
    public class BattlePartyPresenter :BasePresenter
    {
        BattlePartyModel _model = null;
        BattlePartyView _view = null;

        private bool _busy = true;
        public BattlePartyPresenter(BattlePartyView view)
        {
            _view = view;
            _model = new BattlePartyModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetStatusEvent((type) => UpdateStatusCommand(type));
            _view.SetHelpInputInfo("CHARACTER_LIST");
            _view.SetAttributeList(GetListData(_model.AttributeTabList()));
            _view.SetTacticsMembers(GetListData(_model.StageMembers()));
            var enemyInfos = _model.EnemyInfos();
            _view.SetEnemyMembers(GetListData(enemyInfos));
            _view.SetStatusButtonEvent(() => CommandStatusInfo());
            _view.OpenAnimation();
            CommandRefresh();
            _busy = false;
        }

        private void UpdateCommand(BattlePartyViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            switch (viewEvent.commandType)
            {
                case CommandType.DecideTacticsMember:
                    CommandDecideTacticsMember((ActorInfo)viewEvent.template);
                    break;
                case CommandType.SelectTacticsMember:
                    CommandSelectTacticsMember((ActorInfo)viewEvent.template);
                    break;
                case CommandType.EnemyInfo:
                    CommandEnemyInfo();
                    break;
                case CommandType.BattleReplay:
                    CommandBattleReplay();
                    break;
                case CommandType.BattleStart:
                    CommandBattleStart();
                    break;
                case CommandType.CommandHelp:
                    break;
            }
        }

        private void UpdateStatusCommand(StatusViewEvent statusViewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            switch (statusViewEvent.commandType)
            {
                case Status.CommandType.LevelUp:
                    CommandLevelUp();
                    return;
                case Status.CommandType.ShowLearnMagic:
                    CommandShowLearnMagic();
                    return;
                case Status.CommandType.LearnMagic:
                    CommandLearnMagic((SkillInfo)statusViewEvent.template);
                    return;
                case Status.CommandType.HideLearnMagic:
                    CommandHideLearnMagic();
                    return;
                case Status.CommandType.SelectCommandList:
                    CommandSelectSkillTrigger();
                    return;
            }
        }

        private void CommandDecideTacticsMember(ActorInfo actorInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.SetInBattle();
            CommandRefresh();
        }

        private void CommandSelectTacticsMember(ActorInfo actorInfo)
        {
            _model.SetCurrentActorInfo(actorInfo);
        }

        private void CommandStatusInfo()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CommandStatusInfo(_model.StageMembers(),false,true,false,false,_model.CurrentActor.ActorId,() => 
            {
            });
        }

        private void CommandEnemyInfo()
        {
            _busy = true;
            var enemyInfos = _model.EnemyInfos();
            CommandEnemyInfo(enemyInfos,false,() => {_busy = false;});
        }

        private void CommandBattleReplay()
        {
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.ClearParty,
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void CommandBattleStart()
        {
            if (_model.BattleMembers().Count > 0)
            {
                _model.SaveTempBattleMembers();
                _view.CommandChangeViewToTransition(null);
                // ボス戦なら
                if (_model.CurrentSelectRecord().SymbolType == SymbolType.Boss)
                {
                    PlayBossBgm();
                } else
                {
                    var bgmData = DataSystem.Data.GetBGM(_model.TacticsBgmKey());
                    if (bgmData.CrossFade != "" && SoundManager.Instance.CrossFadeMode)
                    {
                        SoundManager.Instance.ChangeCrossFade();
                    } else
                    {
                        PlayTacticsBgm();
                    }
                }
                _model.SetPartyBattlerIdList();
                SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
                var battleSceneInfo = new BattleSceneInfo
                {
                    ActorInfos = _model.BattleMembers(),
                    EnemyInfos = _model.CurrentTroopInfo().BattlerInfos
                };
                _view.CommandSceneChange(Scene.Battle,battleSceneInfo);
            } else
            {
                CheckBattleMember();
            }
        }

        private void CheckBattleMember()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Deny);
            CommandCautionInfo(DataSystem.GetText(19400));
        }

        private void ShowCharacterDetail()
        {
            _view.ShowCharacterDetail(_model.CurrentActor,_model.StageMembers(),_model.SkillActionListData(_model.CurrentActor));  
        }
        
        private void CommandLevelUp()
        {
            _busy = true;
            _view.SetBusy(true);
            CommandLevelUp(_model.CurrentActor,() => 
            {
                _busy = false;
                _view.SetBusy(false);
                CommandRefresh();
            });
        }

        private void CommandShowLearnMagic()
        {
            _view.SetLearnMagicButtonActive(true);
            var lastSelectSkillId = -1;
            var lastSelectSkill = _view.SelectMagic;
            if (lastSelectSkill != null)
            {
                lastSelectSkillId = lastSelectSkill.Id;
            }
            _view.ShowLeaningList(_model.SelectActorLearningMagicList(lastSelectSkillId));
        }

        private void CommandLearnMagic(SkillInfo skillInfo)
        {
            CommandLearnMagic(_model.CurrentActor,skillInfo,() => 
            {
                _view.SetNuminous(_model.Currency);
                //_view.CommandRefresh();
                CommandShowLearnMagic();
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            });
        }

        private void CommandHideLearnMagic()
        {
            _view.SetLearnMagicButtonActive(false);
            CommandRefresh();
        }

        private void CommandSelectSkillTrigger()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = true;
            var skillTriggerViewInfo = new SkillTriggerViewInfo(_model.CurrentActor.ActorId,() => 
            {
                _busy = false;
                CommandRefresh();
            });
            _view.CommandCallSkillTrigger(skillTriggerViewInfo);
        }

        private void CommandRefresh()
        {
            ShowCharacterDetail();
            _view.SetBattleMembers(GetListData(_model.BattleMembers()));
            _view.SetNuminous(_model.Currency);
        }
    }

    public class BattlePartyInfo
    {
        private System.Action<int> _callEvent;
        public System.Action<int> CallEvent => _callEvent;
        public BattlePartyInfo(System.Action<int> callEvent,System.Action backEvent)
        {
            _callEvent = callEvent;
            _backEvent = backEvent;
        }
        private System.Action _backEvent;
        public System.Action BackEvent => _backEvent;
        
        private List<ActorInfo> _actorInfos;
        public List<ActorInfo> ActorInfos => _actorInfos;
        public void SetActorInfos(List<ActorInfo> actorInfos)
        {
            _actorInfos = actorInfos;
        }
    }
}