using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EnemyInfo;
using TMPro;

public class EnemyInfoView : BaseView,IInputHandlerEvent
{
    [SerializeField] private SkillList skillList = null;
    [SerializeField] private EnemyInfoComponent enemyInfoComponent = null;
    [SerializeField] private StatusConditionList statusConditionList = null;
    [SerializeField] private Button skillButton = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject leftRoot = null;
    [SerializeField] private GameObject rightRoot = null;
    [SerializeField] private GameObject leftPrefab = null;
    [SerializeField] private GameObject rightPrefab = null;

    private new System.Action<EnemyInfoViewEvent> _commandData = null;
    private Button _leftButton = null;
    private Button _rightButton = null;
    private System.Action _backEvent = null;
    protected void Awake(){
        InitializeInput();
    }

    public void Initialize(List<BattlerInfo> enemyInfos){
        skillList.Initialize();
        InitializeSkillActionList();
        skillList.InitializeAttribute((attribute) => CallAttributeTypes(attribute));

        GameObject prefab2 = Instantiate(leftPrefab);
        prefab2.transform.SetParent(leftRoot.transform, false);
        _leftButton = prefab2.GetComponent<Button>();
        _leftButton.onClick.AddListener(() => OnClickLeft());
        
        GameObject prefab3 = Instantiate(rightPrefab);
        prefab3.transform.SetParent(rightRoot.transform, false);
        _rightButton = prefab3.GetComponent<Button>();
        _rightButton.onClick.AddListener(() => OnClickRight());

        statusConditionList.Initialize(() => OnClickCondition());
        SetInputHandler(statusConditionList.GetComponent<IInputHandlerEvent>());
        DeactivateConditionList();

        skillButton.onClick.AddListener(() => OnClickSkill());

        new EnemyInfoPresenter(this,enemyInfos);
        SetInputHandler(gameObject.GetComponent<IInputHandlerEvent>());
    }
    private void InitializeSkillActionList()
    {
        skillList.InitializeAction(null,null,null,null,null);
        SetInputHandler(skillList.skillActionList.GetComponent<IInputHandlerEvent>());
        SetInputHandler(skillList.skillAttributeList.GetComponent<IInputHandlerEvent>());
        skillList.HideActionList();
        skillList.HideAttributeList();
    }
    
    private void CallAttributeTypes(AttributeType attributeType)
    {
        var eventData = new EnemyInfoViewEvent(CommandType.AttributeType);
        eventData.templete = attributeType;
        _commandData(eventData);
    }

    private void OnClickLeft()
    {
        if (!_leftButton.gameObject.activeSelf) return;
        var eventData = new EnemyInfoViewEvent(CommandType.LeftActor);
        _commandData(eventData);
    }

    private void OnClickRight()
    {
        if (!_rightButton.gameObject.activeSelf) return;
        var eventData = new EnemyInfoViewEvent(CommandType.RightActor);
        _commandData(eventData);
    }

    private void OnClickCondition()
    {
        var eventData = new EnemyInfoViewEvent(CommandType.Condition);
        _commandData(eventData);
    }

    private void OnClickSkill()
    {
        var eventData = new EnemyInfoViewEvent(CommandType.Skill);
        _commandData(eventData);
    }

    public void SetHelpWindow(){
    }

    public void SetEvent(System.Action<EnemyInfoViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void StartEnemyInfo(BattlerInfo battlerInfo){
        enemyInfoComponent.UpdateInfo(battlerInfo);
    }

    public void RefreshSkillActionList(List<SkillInfo> skillInfos)
    {
        skillList.ShowActionList();
        skillList.HideAttributeList();
        skillList.ActivateActionList();
        skillList.DeactivateAttributeList();
        skillList.SetSkillInfos(skillInfos);
        skillList.RefreshAction();
        HideCondition();
    }

    public void SetAttributeTypes(List<AttributeType> attributeTypes,AttributeType currentAttibuteType)
    {
        skillList.RefreshAttribute(attributeTypes,currentAttibuteType);
    }

    public void SetBackEvent(System.Action backEvent)
    {
        _backEvent = backEvent;
        CreateBackCommand(() => 
        {    
            var eventData = new EnemyInfoViewEvent(CommandType.Back);
            _commandData(eventData);
        });
        SetActiveBack(true);
    }

    public void CommandBack()
    {
        if (_backEvent != null)
        {
            _backEvent();
        }
    }

    public void InputHandler(InputKeyType keyType)
    {

    }

    public void HideSkillActionList()
    {
        skillList.HideActionList();
        skillList.DeactivateActionList();
    }
    
    public void ActivateConditionList()
    {
        statusConditionList.Activate();
    }
    
    public void DeactivateConditionList()
    {
        statusConditionList.Deactivate();
    }

    public void SetCondition(List<StateInfo> stateInfos)
    {
        statusConditionList.Refresh(stateInfos,() => CommandBack(),() => {
            skillList.ShowActionList();
            skillList.ActivateActionList();
            HideCondition();
        });
    }
    
    public void ShowConditionAll()
    {
        statusConditionList.gameObject.SetActive(true);
        statusConditionList.ShowMainView();
    }

    public void HideCondition()
    {
        statusConditionList.HideMainView();
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
        LeftActor,
        RightActor,
        AttributeType,
        Condition,
        Skill
    }
}
public class EnemyInfoViewEvent
{
    public EnemyInfo.CommandType commandType;
    public object templete;

    public EnemyInfoViewEvent(EnemyInfo.CommandType type)
    {
        commandType = type;
    }
}
