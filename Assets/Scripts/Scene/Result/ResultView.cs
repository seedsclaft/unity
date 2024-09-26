using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Result;
using TMPro;
using DG.Tweening;

namespace Ryneus
{
    public class ResultView : BaseView
    {
        [SerializeField] private Image backgroundImage = null; 
        [SerializeField] private StrategyActorList strategyActorList = null; 
        [SerializeField] private CanvasGroup strategyResultCanvasGroup = null;
        [SerializeField] private BaseList strategyResultList = null; 
        [SerializeField] private BaseList commandList = null; 
        [SerializeField] private GameObject animRoot = null;
        [SerializeField] private GameObject animPrefab = null;
        [SerializeField] private GameObject battleScoreObj = null;
        [SerializeField] private TextMeshProUGUI battleScoreText = null; 
        [SerializeField] private GameObject rankingObj = null;
        [SerializeField] private TextMeshProUGUI rankingText = null; 
        private BattleStartAnim _battleStartAnim = null;
        private bool _animationBusy = false;
        public bool AnimationBusy => _animationBusy;

        private new System.Action<ResultViewEvent> _commandData = null;

        public override void Initialize() 
        {
            base.Initialize();

            commandList.Initialize();
            SetInputHandler(commandList.gameObject);
            commandList.gameObject.SetActive(false);
            
            GameObject prefab = Instantiate(animPrefab);
            prefab.transform.SetParent(animRoot.transform, false);
            _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
            _battleStartAnim.gameObject.SetActive(false);

            strategyResultCanvasGroup.alpha = 0;
            battleScoreObj?.SetActive(false);
            rankingObj?.SetActive(false);
            new ResultPresenter(this);
        }

        public void StartLvUpAnimation()
        {
            _battleStartAnim.SetText(DataSystem.GetText(20030));
            _battleStartAnim.StartAnim();
            _battleStartAnim.gameObject.SetActive(true);
            _animationBusy = true;
        }

        public void SetHelpWindow()
        {
            HelpWindow.SetInputInfo("");
            HelpWindow.SetHelpText(DataSystem.GetText(20020));
        }

        public void InitActors()
        {
            strategyActorList.Initialize();
            strategyActorList.gameObject.SetActive(false);
        }

        public void InitResultList(List<ListData> confirmCommands)
        {
            strategyResultList.Initialize();
            SetInputHandler(strategyResultList.gameObject);
            strategyResultList.Deactivate();
            strategyResultCanvasGroup.alpha = 0;
            
            commandList.SetData(confirmCommands);
            commandList.SetInputHandler(InputKeyType.Decide,() => CallResultCommand());
        }

        public void SetEvent(System.Action<ResultViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void StartResultAnimation(List<ListData> actorInfos,List<bool> isBonusList = null)
        {
            DeactivateAll();
            strategyActorList.gameObject.SetActive(false);
            strategyActorList.SetData(actorInfos);
            strategyActorList.StartResultAnimation(actorInfos.Count,isBonusList,() => 
            {
                CallEndAnimation();
            });
            strategyActorList.gameObject.SetActive(true);
        }

        private void CallEndAnimation()
        {
            var eventData = new ResultViewEvent(CommandType.EndAnimation);
            _commandData(eventData);
        }

        public void ShowResultList(List<ListData> getItemInfos,string battleScore)
        {
            strategyResultCanvasGroup.alpha = 1;
            strategyResultList.gameObject.SetActive(true);
            strategyResultList.SetData(getItemInfos);
            strategyResultList.Activate();
            battleScoreObj?.SetActive(battleScore != null);
            battleScoreText?.SetText(battleScore);
            commandList.gameObject.SetActive(true);
            commandList.Activate();
            SetHelpInputInfo("STRATEGY");
        }

        public void SetRanking(string ranking)
        {
            rankingObj?.SetActive(true);
            rankingText?.SetText(ranking);
        }

        private void CallResultCommand()
        {
            var data = commandList.ListItemData<SystemData.CommandData>();
            if (data != null)
            {
                var eventData = new ResultViewEvent(CommandType.ResultClose)
                {
                    template = data
                };
                _commandData(eventData);
            }
        }

        private new void Update() 
        {
            if (_animationBusy == true)
            {
                CheckAnimationBusy();
                return;
            }
            base.Update();
        }

        private void CheckAnimationBusy()
        {
            if (_battleStartAnim.IsBusy == false)
            {
                _animationBusy = false;
                var eventData = new ResultViewEvent(CommandType.EndLvUpAnimation);
                _commandData(eventData);
            }
        }

        private void DeactivateAll()
        {
            strategyResultList.Deactivate();
            commandList.Deactivate();
        }
    }

    public class ResultViewEvent
    {
        public CommandType commandType;
        public object template;

        public ResultViewEvent(CommandType type)
        {
            commandType = type;
        }
    }
}


namespace Result
{
    public enum CommandType
    {
        None = 0,
        StartResult = 1,
        EndAnimation = 2,
        ResultClose = 5,
        EndLvUpAnimation = 9,
    }
}