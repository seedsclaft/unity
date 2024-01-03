using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EnemyInfo;
using Effekseer;

public class EnemyInfoView : BaseView,IInputHandlerEvent
{
    [SerializeField] private BattleSelectCharacter selectCharacter = null;
    [SerializeField] private BattleEnemyLayer enemyLayer = null;
    [SerializeField] private EnemyInfoComponent enemyInfoComponent = null;
    private new System.Action<EnemyInfoViewEvent> _commandData = null;
    [SerializeField] private Button leftButton = null;
    [SerializeField] private Button rightButton = null;
    private System.Action _backEvent = null;

    private bool _isBattle = false;
    protected void Awake(){
        InitializeInput();
    }

    public void Initialize(List<BattlerInfo> battlerInfos,List<EffekseerEffectAsset> cursorEffects,bool isBattle){
        _isBattle = isBattle;
        
        enemyLayer.Initialize(battlerInfos,(a) => OnClickSelectEnemy(a),cursorEffects);
        enemyLayer.SetSelectedHandler(() => OnSelectEnemy());
        selectCharacter.Initialize();
        SetInputHandler(selectCharacter.GetComponent<IInputHandlerEvent>());
        InitializeSelectCharacter();
/*
        leftButton.onClick.AddListener(() => OnClickLeft());
        rightButton.onClick.AddListener(() => OnClickRight());
*/
        new EnemyInfoPresenter(this,battlerInfos);
        SetInputHandler(gameObject.GetComponent<IInputHandlerEvent>());
    }

    private void InitializeSelectCharacter()
    {
        selectCharacter.SetInputHandlerAction(InputKeyType.Decide,() => {});
        selectCharacter.SetInputHandlerAction(InputKeyType.Cancel,() => OnClickBack());
        selectCharacter.SetInputHandlerAction(InputKeyType.SideLeft1,() => OnClickLeft());
        selectCharacter.SetInputHandlerAction(InputKeyType.SideRight1,() => OnClickRight());
        selectCharacter.SetInputHandlerAction(InputKeyType.SideLeft2,() => {
            selectCharacter.SelectCharacterTabSmooth(-1);
        });
        selectCharacter.SetInputHandlerAction(InputKeyType.SideRight2,() => {
            selectCharacter.SelectCharacterTabSmooth(1);
        });
        SetInputHandler(selectCharacter.MagicList.GetComponent<IInputHandlerEvent>());
        selectCharacter.HideActionList();
        selectCharacter.SelectCharacterTabSmooth(0);
    }

    public void CommandRefreshStatus(List<ListData> skillInfos,BattlerInfo battlerInfo,List<int> enemyIndexes,int lastSelectIndex)
    {
        selectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
        selectCharacter.ShowActionList();
        selectCharacter.UpdateStatus(battlerInfo);
        selectCharacter.SetSkillInfos(skillInfos);
        selectCharacter.RefreshAction(lastSelectIndex);
        enemyLayer.RefreshTarget(battlerInfo.Index,enemyIndexes,ScopeType.One);
        enemyInfoComponent.Clear();
        enemyInfoComponent.UpdateInfo(battlerInfo);
    }

    private void OnClickBack()
    {
        var eventData = new EnemyInfoViewEvent(CommandType.Back);
        _commandData(eventData);
    }

    private void OnClickLeft()
    {
        if (!leftButton.gameObject.activeSelf) return;
        var eventData = new EnemyInfoViewEvent(CommandType.LeftEnemy);
        _commandData(eventData);
    }

    private void OnClickRight()
    {
        if (!rightButton.gameObject.activeSelf) return;
        var eventData = new EnemyInfoViewEvent(CommandType.RightEnemy);
        _commandData(eventData);
    }

    private void OnClickSelectEnemy(List<int> enemyIndex)
    {
        if (enemyIndex.Count > 0)
        {
            var eventData = new EnemyInfoViewEvent(CommandType.SelectEnemy);
            eventData.template = enemyIndex[0];
            _commandData(eventData);
        }
    }
    
    private void OnSelectEnemy()
    {
        var enemyIndex = enemyLayer.Index;
        if (enemyIndex > -1)
        {
            var eventData = new EnemyInfoViewEvent(CommandType.SelectEnemy);
            eventData.template = enemyIndex;
            _commandData(eventData);
        }
    }

    public void SetHelpWindow()
    {
        HelpWindow.SetHelpText(DataSystem.GetTextData(809).Help);
        if (_isBattle)
        {
            HelpWindow.SetInputInfo("ENEMYINFO_BATTLE");
        } else
        {
            HelpWindow.SetInputInfo("ENEMYINFO");
        }
    }

    public void SetEvent(System.Action<EnemyInfoViewEvent> commandData)
    {
        _commandData = commandData;
    }


    public void SetCondition(List<ListData> skillInfos)
    {
        selectCharacter.SetConditionList(skillInfos);
    }

    public void SetBackEvent(System.Action backEvent)
    {
        _backEvent = backEvent;
        SetBackCommand(() => 
        {    
            var eventData = new EnemyInfoViewEvent(CommandType.Back);
            _commandData(eventData);
        });
        ChangeBackCommandActive(true);
    }

    public void CommandBack()
    {
        if (_backEvent != null)
        {
            _backEvent();
        }
    }

    public void InputHandler(InputKeyType keyType,bool pressed)
    {
        if (keyType == InputKeyType.SideLeft1)
        {
            if (_isBattle)
            {
            } else
            {
                OnClickLeft();
            }
        }
        if (keyType == InputKeyType.SideRight1)
        {
            if (_isBattle)
            {
            } else
            {
                OnClickRight();
            }
        }
    }


    public new void MouseCancelHandler()
    {
        CommandBack();
    }
}

namespace EnemyInfo
{
    public enum CommandType
    {
        None = 0,
        Back,
        LeftEnemy,
        RightEnemy,
        SelectEnemy,
    }
}
public class EnemyInfoViewEvent
{
    public EnemyInfo.CommandType commandType;
    public object template;

    public EnemyInfoViewEvent(EnemyInfo.CommandType type)
    {
        commandType = type;
    }
}
