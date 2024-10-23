using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Strategy;
using TMPro;
using DG.Tweening;

namespace Ryneus
{
    public class StrategyView : BaseView
    {
        [SerializeField] private Image backgroundImage = null; 
        [SerializeField] private StrategyActorList strategyActorList = null; 
        [SerializeField] private CanvasGroup strategyResultCanvasGroup = null;
        [SerializeField] private BaseList strategyResultList = null; 
        public bool StrategyResultListActive => strategyResultList.gameObject.activeSelf; 
        [SerializeField] private BaseList commandList = null; 
        [SerializeField] private BaseList statusList = null; 
        [SerializeField] private MagicList alcanaSelectList = null;
        [SerializeField] private TextMeshProUGUI title = null; 
        [SerializeField] private ActorInfoComponent actorInfoComponent = null;
        [SerializeField] private Button lvUpStatusButton = null;
        [SerializeField] private GameObject animRoot = null;
        [SerializeField] private GameObject animPrefab = null;
        [SerializeField] private GameObject saveHumanObj = null;
        [SerializeField] private TextMeshProUGUI saveHumanText = null; 
        [SerializeField] private GameObject battleTurnObj = null;
        [SerializeField] private TextMeshProUGUI battleTurnText = null; 
        [SerializeField] private GameObject battleScoreObj = null;
        [SerializeField] private TextMeshProUGUI battleScoreText = null; 
        [SerializeField] private GameObject battleMaxDamageObj = null;
        [SerializeField] private TextMeshProUGUI battleMaxDamageText = null; 
        [SerializeField] private GameObject battleAttackPerObj = null;
        [SerializeField] private TextMeshProUGUI battleAttackPerText = null; 
        [SerializeField] private GameObject battleDefeatedCountObj = null;
        [SerializeField] private TextMeshProUGUI battleDefeatedCountText = null; 

        private BattleStartAnim _battleStartAnim = null;
        private bool _animationBusy = false;
        public bool AnimationBusy => _animationBusy;
        public int AlcanaListIndex => alcanaSelectList.Index;

        private new System.Action<StrategyViewEvent> _commandData = null;

        public override void Initialize() 
        {
            base.Initialize();
            statusList.Initialize();
            statusList.SetInputHandler(InputKeyType.Decide,() => CallLvUpNext());
            SetInputHandler(statusList.gameObject);

            commandList.Initialize();
            SetInputHandler(commandList.gameObject);
            commandList.gameObject.SetActive(false);
            
            GameObject prefab = Instantiate(animPrefab);
            prefab.transform.SetParent(animRoot.transform, false);
            _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
            _battleStartAnim.gameObject.SetActive(false);
            lvUpStatusButton.onClick.AddListener(() => CallLvUpNext());
            lvUpStatusButton.gameObject.SetActive(false);
            
            var rect = actorInfoComponent.gameObject.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(0,0,0);
            actorInfoComponent.MainThumb.DOFade(0,0);

            strategyResultCanvasGroup.alpha = 0;
            saveHumanObj?.SetActive(false);
            battleTurnObj?.SetActive(false);
            battleScoreObj?.SetActive(false);
            battleMaxDamageObj?.SetActive(false);
            battleAttackPerObj?.SetActive(false);
            battleDefeatedCountObj?.SetActive(false);
            alcanaSelectList.Initialize();
            alcanaSelectList.Hide();
            new StrategyPresenter(this);
        }

        private void CallLvUpNext()
        {
            lvUpStatusButton.gameObject.SetActive(false);
            actorInfoComponent.gameObject.SetActive(false);
            statusList.gameObject.SetActive(false);
            var eventData = new StrategyViewEvent(CommandType.LvUpNext);
            _commandData(eventData);
        }

        public void StartLvUpAnimation()
        {
            _battleStartAnim.SetText(DataSystem.GetText(20030));
            _battleStartAnim.StartAnim(false);
            _battleStartAnim.gameObject.SetActive(true);
            _animationBusy = true;
        }

        public void ShowLvUpActor(ActorInfo actorInfo,List<ListData> status)
        {
            strategyResultList.Deactivate();
            statusList.gameObject.SetActive(true);
            statusList.Activate();
            lvUpStatusButton.gameObject.SetActive(true);
            actorInfoComponent.gameObject.SetActive(true);
            actorInfoComponent.Clear();
            actorInfoComponent.UpdateInfo(actorInfo,null);
            
            var rect = actorInfoComponent.gameObject.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(0,0,0);
            actorInfoComponent.MainThumb.DOFade(0,0);
            
            BaseAnimation.MoveAndFade(rect,actorInfoComponent.MainThumb,24,1);

            statusList.SetData(status);
            HelpWindow.SetInputInfo("LEVELUP");
        }

        public void SetTitle(string text)
        {
            title.text = text;
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
            
            commandList.SetData(confirmCommands,true,() =>
            {
                commandList.UpdateSelectIndex(1);
            });
            commandList.SetInputHandler(InputKeyType.Decide,() => CallResultCommand());
        }

        public void SetEvent(System.Action<StrategyViewEvent> commandData)
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
            var eventData = new StrategyViewEvent(CommandType.EndAnimation);
            _commandData(eventData);
        }

        public void ShowResultList(List<ListData> getItemInfos,string saveHuman = null,string battleTurn = null,string battleScore = null,string maxDamage = null,string attackPer = null,string defeatedCount = null)
        {
            strategyResultCanvasGroup.alpha = 1;
            strategyResultList.gameObject.SetActive(true);
            strategyResultList.SetData(getItemInfos);
            strategyResultList.Activate();
            saveHumanObj?.SetActive(saveHuman != null);
            battleTurnObj?.SetActive(battleTurn != null);
            battleScoreObj?.SetActive(battleScore != null);
            battleMaxDamageObj?.SetActive(maxDamage != null);
            battleAttackPerObj?.SetActive(attackPer != null);
            battleDefeatedCountObj?.SetActive(defeatedCount != null);
            saveHumanText?.SetText(saveHuman);
            battleTurnText?.SetText(battleTurn);
            battleScoreText?.SetText(battleScore);
            battleMaxDamageText?.SetText(maxDamage);
            battleDefeatedCountText?.SetText(defeatedCount);
            battleAttackPerText?.SetText(attackPer);
            commandList.gameObject.SetActive(true);
            commandList.Activate();
            SetHelpInputInfo("STRATEGY");
        }

        private void CallResultCommand()
        {
            var data = commandList.ListItemData<SystemData.CommandData>();
            if (data != null)
            {
                var eventData = new StrategyViewEvent(CommandType.ResultClose)
                {
                    template = data
                };
                _commandData(eventData);
            }
        }

        public void HideResultList()
        {
            strategyResultList.Deactivate();
            strategyResultList.gameObject.SetActive(false);
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
                var eventData = new StrategyViewEvent(CommandType.EndLvUpAnimation);
                _commandData(eventData);
            }
        }

        public void EndShinyEffect()
        {
            strategyActorList.SetShinyReflect(false);
        }

        public void FadeOut()
        {
            backgroundImage.DOFade(0,0.4f);
        }

        private void DeactivateAll()
        {
            strategyResultList.Deactivate();
            commandList.Deactivate();
        }

        public void HideAlcanaList()
        {
            alcanaSelectList.Hide();
        }

        public void SetAlcanaSelectInfos(List<ListData> skillInfos)
        {
            SetBackEvent(() => {});
            alcanaSelectList.SetData(skillInfos);
            alcanaSelectList.SetInputHandler(InputKeyType.Decide,() => 
            {
                if (AlcanaSelectSkillInfo() != null)
                {
                    var eventData = new StrategyViewEvent(CommandType.SelectAlcanaList)
                    {
                        template = AlcanaSelectSkillInfo()
                    };
                    _commandData(eventData);
                }
            });
            alcanaSelectList.Show();
        }

        public SkillInfo AlcanaSelectSkillInfo() 
        {
            var data = alcanaSelectList.ListItemData<SkillInfo>();
            if (data != null)
            {
                if (data != null && data.Enable)
                {
                    return data;
                }
            }
            return null;
        }
    }

    public class StrategyViewEvent
    {
        public CommandType commandType;
        public object template;

        public StrategyViewEvent(CommandType type)
        {
            commandType = type;
        }
    }
}


namespace Strategy
{
    public enum CommandType
    {
        None = 0,
        StartStrategy = 1,
        EndAnimation = 2,
        PopupSkillInfo = 3,
        CallEnemyInfo = 4,
        ResultClose = 5,
        LvUpNext = 7,
        SelectAlcanaList = 8,
        EndLvUpAnimation = 9,
    }
}