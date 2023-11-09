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
    [SerializeField] private List<Toggle> detailTabs;
    [SerializeField] private List<GameObject> detailObjs;
    [SerializeField] private List<CanvasGroup> detailTabCanvasGroup;
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
        foreach (var magicConditionTab in detailTabs)
        {
            var tabIndex = idx;
            magicConditionTab.onValueChanged.AddListener((a) => 
            {
                SelectMagicConditionTab((SelectCharacterTabType)tabIndex);
            });
            idx++;
        }
        gameObject.SetActive(false);
        displaySelectCard.Clear();
    }

    private void SelectMagicConditionTab(SelectCharacterTabType selectCharacterTabType)
    {
        if (_selectCharacterTabType == selectCharacterTabType)
        {
            return;
        }
        _selectCharacterTabType = selectCharacterTabType;
        UpdateTabs();
        if (selectCharacterTabType == SelectCharacterTabType.Magic)
        {
            deckMagicList.Activate();
            conditionList.Deactivate();
        } else
        if (selectCharacterTabType == SelectCharacterTabType.Condition)
        {
            deckMagicList.Deactivate();
            conditionList.Activate();
        } else
        if (selectCharacterTabType == SelectCharacterTabType.Detail)
        {
            deckMagicList.Deactivate();
            conditionList.Deactivate();
        }
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
    }

    private void UpdateTabs()
    {
        for (int i = 0;i < detailTabs.Count;i++)
        {
            detailTabs[i].SetIsOnWithoutNotify((int)_selectCharacterTabType == i);
        }
        for (int i = 0;i < detailObjs.Count;i++)
        {
            detailObjs[i].SetActive((int)_selectCharacterTabType == i);
        }
        for (int i = 0;i < detailTabCanvasGroup.Count;i++)
        {
            detailTabCanvasGroup[i].alpha = (int)_selectCharacterTabType == i ? 1 : 0.25f;
        }
    }
    
    public void SetActiveTab(SelectCharacterTabType selectCharacterTabType,bool isActive)
    {    
        detailTabs[(int)selectCharacterTabType].gameObject.SetActive(isActive);
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
