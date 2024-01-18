using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleSelectCharacter : MonoBehaviour
{   
    [SerializeField] private SkillInfoComponent displaySelectCard;
    [SerializeField] private BaseList magicList;
    public BaseList MagicList => magicList;
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
            return (SkillInfo)magicList.ListData.Data;
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
                SelectCharacterTab((SelectCharacterTabType)tabIndex);
            });
            idx++;
        }
        gameObject.SetActive(false);
        displaySelectCard.Clear();
        UpdateTabs();
    }

    private void SelectCharacterTab(SelectCharacterTabType selectCharacterTabType)
    {
        if (_selectCharacterTabType == selectCharacterTabType)
        {
            return;
        }
        _selectCharacterTabType = selectCharacterTabType;
        UpdateTabs();
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
    }

    public void SelectCharacterTabSmooth(int index)
    {
        var nextIndex = (int)_selectCharacterTabType + index;
        var displayTabs = detailTabs.FindAll(a => a.gameObject.activeSelf);
        if (nextIndex < 0)
        {
            for (int i = 0;i < detailTabs.Count;i++)
            {
                if (detailTabs[i].gameObject.activeSelf)
                {            
                    nextIndex = i;
                }
            }
        } else
        if (nextIndex > displayTabs.Count)
        {
            nextIndex = detailTabs.FindIndex(a => a.gameObject.activeSelf);
        } else
        {
            if (index > 0)
            {
                for (int i = 0;i < detailTabs.Count;i++)
                {
                    if (!detailTabs[i].gameObject.activeSelf && (i == nextIndex))
                    {
                        nextIndex++;
                    }
                }
            } else
            {
                
                for (int i = detailTabs.Count-1;i >= 0;i--)
                {
                    if (!detailTabs[i].gameObject.activeSelf && (i == nextIndex))
                    {
                        nextIndex--;
                    }
                }
            }
        }
        SelectCharacterTab((SelectCharacterTabType)nextIndex);
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

    public void UpdateStatus(ActorInfo actorInfo)
    {
        var currentStatus = actorInfo.CurrentStatus;
        statusInfoComponent.UpdateInfo(currentStatus);
        statusInfoComponent.UpdateHp(actorInfo.MaxHp,currentStatus.Hp);
        statusInfoComponent.UpdateMp(actorInfo.MaxMp,currentStatus.Mp);
    }

    public void UpdateStatus(BattlerInfo battlerInfo)
    {
        var baseStatus = battlerInfo.CurrentStatus(true);
        var currentStatus = battlerInfo.CurrentStatus(false);
        statusInfoComponent.UpdateInfo(currentStatus,baseStatus);
        statusInfoComponent.UpdateHp(battlerInfo.MaxHp,currentStatus.Hp);
        statusInfoComponent.UpdateMp(battlerInfo.MaxMp,currentStatus.Mp);
    }

    public void HideStatus()
    {
        statusInfoComponent.gameObject.SetActive(false);
    }

    public void SetSkillInfos(List<ListData> skillInfoData)
    {
        if (magicList.IsInit == false)
        {
            magicList.Initialize(skillInfoData.Count);
            magicList.SetSelectedHandler(() => {
                DisplaySelectCard();
                RefreshCardWidth();
            });
        }
        magicList.SetData(skillInfoData);
        if (displaySelectCard == null)
        {
            displaySelectCard.gameObject.SetActive(false);
        }
        SelectCharacterTab(_selectCharacterTabType);
        magicList.UpdateSelectIndex(skillInfoData.Count > 0 ? 0 : -1);
        DisplaySelectCard();
        RefreshCardWidth();
    }

    public void SetConditionList(List<ListData> conditionData)
    {
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
        var listData = magicList.ListData;
        if (listData != null)
        {
            var skillInfo = (SkillInfo)listData.Data;
            if (skillInfo != null)
            {
                displaySelectCard.gameObject.SetActive(true);
                displaySelectCard.UpdateSkillInfo(skillInfo);
            }
        }
    }

    public void SetInputHandlerAction(InputKeyType keyType,System.Action callEvent)
    {
        magicList.SetInputHandler(keyType,callEvent);
        conditionList.SetInputHandler(keyType,callEvent);
    }

    public void RefreshAction(int selectIndex = 0)
    {
        magicList.Refresh(selectIndex);
    }

    public void RefreshCostInfo()
    {
        magicList.UpdateAllItems();
    }

    public void ShowActionList()
    {
        gameObject.SetActive(true);
        magicList.Activate();
        conditionList.Deactivate();
    }

    public void HideActionList()
    {
        gameObject.SetActive(false);
        magicList.Deactivate();
        conditionList.Deactivate();
    }

    public void RefreshCardWidth()
    {
        if (magicList.ObjectList.Count == 0 || magicList.Index < 0)
        {
            return;
        }
        if (magicList.ObjectList.Count > magicList.Index)
        {
            var selectObj = magicList.ObjectList[magicList.Index];
            foreach (var gameObject in magicList.ObjectList)
            {
                var rect = gameObject.GetComponent<RectTransform>();
                var cardWidth = (selectObj != gameObject) ? 184 : 264;
                rect.sizeDelta = new Vector2(cardWidth,240);
            }
            selectObj.SetActive(false);
            selectObj.SetActive(true);
        }
        var listRect = magicList.ScrollRect.gameObject.GetComponent<RectTransform>();
        var height = 8;
        if (magicList.ObjectList.Count < 10)
        {
            height = 0;
        }
        listRect.localPosition = new Vector3(listRect.localPosition.x,height,listRect.localPosition.z);
    }
    
}

public enum SelectCharacterTabType{
    Magic = 0,
    Condition = 1,
    Detail = 2
}
