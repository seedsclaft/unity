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

    private bool _isInit = false;
    
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
        displaySelectCard.gameObject.SetActive(tabIndex == 0);
        conditionList.gameObject.SetActive(tabIndex == 1);
        magicConditionTabs[0].SetIsOnWithoutNotify(tabIndex == 0);
        magicConditionTabs[1].SetIsOnWithoutNotify(tabIndex == 1);
        if (tabIndex == 0)
        {
            deckMagicList.Activate();
            conditionList.Deactivate();
        } else
        {
            deckMagicList.Deactivate();
            conditionList.Activate();
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
        battleThumb.ShowActorThumb(actorInfo,false);
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

    private void DisplaySelectCard()
    {
        if (displaySelectCard == null)
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
