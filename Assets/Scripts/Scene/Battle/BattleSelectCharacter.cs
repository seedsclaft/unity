using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{

    public class BattleSelectCharacter : MonoBehaviour
    {   
        [SerializeField] private SkillInfoComponent displaySelectCard;
        [SerializeField] private BaseList magicList;
        public BaseList MagicList => magicList;
        [SerializeField] private BaseList conditionList;
        [SerializeField] private ToggleSelect toggleSelect;

        [SerializeField] private StatusInfoComponent statusInfoComponent;
        [SerializeField] private ActorInfoComponent actorInfoComponent;
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        [SerializeField] private SideMenuButton lvResetButton;
        private bool _isInit = false;

        public int SelectedTabIndex => toggleSelect.SelectTabIndex;
        
        public SkillInfo ActionData
        {
            get 
            {
                if (magicList.ListData != null)
                {
                    return (SkillInfo)magicList.ListData.Data;
                }
                return null;
            }
        }

        public void Initialize()
        {
            if (_isInit == true)
            {
                return;
            }
            conditionList.Initialize();
            _isInit = true;
            toggleSelect.Initialize(new List<string>()
            {
                DataSystem.GetText(421),
                DataSystem.GetText(420),
                DataSystem.GetText(402)
            });
            toggleSelect.SetClickHandler(() => 
            {
                if (toggleSelect.SelectTabIndex == (int)SelectCharacterTabType.Magic)
                {
                    DisplaySelectCard();
                }
            });
            gameObject.SetActive(false);
            lvResetButton?.gameObject.SetActive(false);
            displaySelectCard.Clear();
            UpdateTabs();
        }

        public void InitializeLvReset(System.Action lvResetEvent)
        {
            lvResetButton?.gameObject.SetActive(true);
            lvResetButton?.SetCallHandler(() => {
                if (lvResetEvent != null) lvResetEvent(); 
            });
        }

        public void SelectCharacterTab(int tabIndex)
        {
            toggleSelect.SetSelectTabIndex(tabIndex);
        }

        public void SelectCharacterTabSmooth(int index)
        {
            toggleSelect.SelectCharacterTabSmooth(index);
        }

        private void UpdateTabs()
        {
            toggleSelect.UpdateTabs();
        }
        
        public void SetActiveTab(SelectCharacterTabType selectCharacterTabType,bool isActive)
        {    
            toggleSelect.SetActiveTab((int)selectCharacterTabType,isActive);
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
            battlerInfoComponent.UpdateInfo(battlerInfo);
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
                magicList.Initialize();
                magicList.SetSelectedHandler(() => {
                    DisplaySelectCard();
                });
            }
            magicList.SetData(skillInfoData);
            if (displaySelectCard == null)
            {
                displaySelectCard.gameObject.SetActive(false);
            }
            SelectCharacterTab(toggleSelect.SelectTabIndex);
            if (skillInfoData.Count == 0)
            {
                magicList.UpdateSelectIndex(-1);
            }
            DisplaySelectCard();
        }

        public void SetConditionList(List<ListData> conditionData)
        {
            conditionList.SetData(conditionData);
        }

        public void SetActorInfo(ActorInfo actorInfo,List<ActorInfo> party)
        {
            actorInfoComponent.UpdateInfo(actorInfo,party);
        }

        public void SetEnemyBattlerInfo(BattlerInfo enemyInfo)
        {
            battlerInfoComponent.UpdateInfo(enemyInfo);
        }

        private void DisplaySelectCard()
        {
            if (displaySelectCard == null)
            {
                return;
            }
            if (toggleSelect.SelectTabIndex != (int)SelectCharacterTabType.Magic)
            {
                return;
            }
            displaySelectCard.Clear();
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
    }

    public enum SelectCharacterTabType{
        Detail = 0,
        Magic = 1,
        Condition = 2,
    }
}