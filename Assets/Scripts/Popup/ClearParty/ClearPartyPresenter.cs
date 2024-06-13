using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class ClearPartyPresenter : BasePresenter
    {
        ClearPartyModel _model = null;
        ClearPartyView _view = null;

        private bool _busy = true;
        public ClearPartyPresenter(ClearPartyView view)
        {
            _view = view;
            _model = new ClearPartyModel();

            Initialize();
        }

        private async void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetHelpInputInfo("CHARACTER_LIST");
            _view.CommandGameSystem(Base.CommandType.CallLoading);
            var saveData = await _model.ClearParty();
            _view.CommandGameSystem(Base.CommandType.CloseLoading);
            _view.SetClearParty(saveData);
            _view.SetBattleReplayEvent((a) => 
            {
                _view.CommandGameSystem(Base.CommandType.ClosePopup);
                _model.SetInReplay(a);
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
                //_model.SetPartyBattlerIdList();
                SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
                _view.CommandSceneChange(Scene.Battle);
            });
        }

        private void UpdateCommand(ClearPartyViewEvent viewEvent)
        {
            if (_busy)
            {
                return;
            }
        }
    }

    public class ClearPartyInfo
    {
        private System.Action<int> _callEvent;
        public System.Action<int> CallEvent => _callEvent;
        public ClearPartyInfo(System.Action<int> callEvent,System.Action backEvent)
        {
            _callEvent = callEvent;
            _backEvent = backEvent;
        }
        private System.Action _backEvent;
        public System.Action BackEvent => _backEvent;
        
    }
}