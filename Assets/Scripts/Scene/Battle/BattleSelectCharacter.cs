using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleSelectCharacter : MonoBehaviour
{   
    [SerializeField] private SkillInfoComponent displaySelectCard;
    [SerializeField] private BattleThumb battleThumb;
    [SerializeField] private BaseList deckMagicList;
    public BaseList DeckMagicList => deckMagicList;
    [SerializeField] private BaseList conditionList;
    [SerializeField] private List<Toggle> magicConditionTabs;
    [SerializeField] private StatusInfoComponent statusInfoComponent;
    [SerializeField] private GameObject detailObj;
    [SerializeField] private ActorInfoComponent actorInfoComponent;

    private bool _isInit = false;

    private SelectCharacterTabType _selectCharacterTabType = SelectCharacterTabType.Magic;
    
    public SkillInfo ActionData{
        get {
            return (SkillInfo)deckMagicList.ListData.Data;
        }
    }


    public void Initialize()
    {
        if (_isInit == true)
        {
            return;
        }
        _isInit = true;
        var idx = 0;
        foreach (var magicConditionTab in magicConditionTabs)
        {
            var tabIndex = idx;
            magicConditionTab.onValueChanged.AddListener((a) => 
            {
                SelectMagicConditionTab(tabIndex);
            });
            idx++;
        }
        gameObject.SetActive(false);
    }

    private void SelectMagicConditionTab(int tabIndex)
    {
        _selectCharacterTabType = (SelectCharacterTabType)tabIndex;
        displaySelectCard.gameObject.SetActive(tabIndex == (int)SelectCharacterTabType.Magic);
        conditionList.gameObject.SetActive(tabIndex == (int)SelectCharacterTabType.Condition);
        detailObj.gameObject.SetActive(tabIndex == (int)SelectCharacterTabType.Detail);
        magicConditionTabs[0].SetIsOnWithoutNotify(tabIndex == (int)SelectCharacterTabType.Magic);
        magicConditionTabs[1].SetIsOnWithoutNotify(tabIndex == (int)SelectCharacterTabType.Condition);
        magicConditionTabs[2].SetIsOnWithoutNotify(tabIndex == (int)SelectCharacterTabType.Detail);
        if (tabIndex == (int)SelectCharacterTabType.Magic)
        {
            deckMagicList.Activate();
            conditionList.Deactivate();
        } else
        if (tabIndex == (int)SelectCharacterTabType.Condition)
        {
            deckMagicList.Deactivate();
            conditionList.Activate();
        } else
        if (tabIndex == (int)SelectCharacterTabType.Detail)
        {
            deckMagicList.Deactivate();
            conditionList.Deactivate();
        }
    }

    public void SetBattleThumb(BattlerInfo battlerInfo)
    {
        battleThumb.ShowBattleThumb(battlerInfo);
        battleThumb.gameObject.SetActive(true);
        var currentStatus = battlerInfo.CurrentStatus();
        statusInfoComponent.UpdateInfo(currentStatus);
        statusInfoComponent.UpdateHp(battlerInfo.Hp,currentStatus.Hp);
        statusInfoComponent.UpdateMp(battlerInfo.Mp,currentStatus.Mp);
    }

    public void SetActorThumb(ActorInfo actorInfo)
    {
        battleThumb.ShowAllThumb(actorInfo);
        battleThumb.gameObject.SetActive(true);
        var currentStatus = actorInfo.CurrentStatus;
        statusInfoComponent.UpdateInfo(currentStatus);
        statusInfoComponent.UpdateHp(actorInfo.MaxHp,currentStatus.Hp);
        statusInfoComponent.UpdateMp(actorInfo.MaxMp,currentStatus.Mp);
    }

    public void HideThumb()
    {
        battleThumb.gameObject.SetActive(false);
    }

    public void SetSkillInfos(List<ListData> skillInfoData)
    {
        if (deckMagicList.IsInit == false)
        {
            deckMagicList.Initialize(skillInfoData.Count);
            deckMagicList.SetSelectedHandler(() => DisplaySelectCard());
        }
        deckMagicList.SetData(skillInfoData);
        if (displaySelectCard == null)
        {
            displaySelectCard.gameObject.SetActive(false);
        }
        SelectMagicConditionTab(0);
        deckMagicList.UpdateSelectIndex(skillInfoData.Count > 0 ? 0 : -1);
        DisplaySelectCard();
    }

    public void SetConditionList(List<ListData> conditionData)
    {
        conditionList.Initialize(conditionData.Count);
        conditionList.SetData(conditionData);
    }

    public void SetActorInfo(ActorInfo actorInfo,List<ActorInfo> party)
    {
        actorInfoComponent.UpdateInfo(actorInfo,party);
    }

    private void DisplaySelectCard()
    {
        if (displaySelectCard == null)
        {
            return;
        }
        if (_selectCharacterTabType != SelectCharacterTabType.Magic)
        {
            return;
        }
        var listData = deckMagicList.ListData;
        if (listData != null)
        {
            var skillInfo = (SkillInfo)listData.Data;
            if (skillInfo != null)
            {
                displaySelectCard.gameObject.SetActive(true);
                displaySelectCard.UpdateSkillData(skillInfo.Id);
            }
        }
    }

    public void SetInputHandlerAction(InputKeyType keyType,System.Action callEvent)
    {
        deckMagicList.SetInputHandler(keyType,callEvent);
        conditionList.SetInputHandler(keyType,callEvent);
    }

    public void RefreshAction(int selectIndex = 0)
    {
        deckMagicList.Refresh(selectIndex);
    }


    public void RefreshCostInfo()
    {
        deckMagicList.UpdateAllItems();
    }

    public void ShowActionList()
    {
        gameObject.SetActive(true);
        deckMagicList.Activate();
        conditionList.Deactivate();
    }

    public void HideActionList()
    {
        gameObject.SetActive(false);
        deckMagicList.Deactivate();
        conditionList.Deactivate();
    }
    
}

public enum SelectCharacterTabType{
    Magic = 0,
    Condition = 1,
    Detail = 2
}
