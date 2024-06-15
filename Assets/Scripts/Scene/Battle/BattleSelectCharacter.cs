using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{

    public class BattleSelectCharacter : BaseView
    {   
        [SerializeField] private MagicList magicList;
        public MagicList MagicList => magicList;
        [SerializeField] private BaseList conditionList;
        [SerializeField] private BaseList attributeList = null;
        public BaseList AttributeList => attributeList;
        [SerializeField] private ToggleSelect toggleSelect;

        [SerializeField] private StatusInfoComponent statusInfoComponent;
        [SerializeField] private ActorInfoComponent actorInfoComponent;
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        [SerializeField] private OnOffButton lvResetButton;
        [SerializeField] private SkillTriggerList skillTriggerList;
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

        public new void Initialize()
        {
            if (_isInit == true)
            {
                return;
            }
            base.Initialize();
            magicList.Initialize();
            SetInputHandler(magicList.gameObject);
            SetBusy(true);
            conditionList.Initialize();
            skillTriggerList?.Initialize();
            attributeList?.Initialize();
            //SetInputHandler(attributeList.GetComponent<IInputHandlerEvent>());
            attributeList?.gameObject.SetActive(false);
            _isInit = true;
            toggleSelect.Initialize(new List<string>()
            {
                DataSystem.GetText(421),
                DataSystem.GetText(420),
                DataSystem.GetText(402),
                DataSystem.GetText(422),
            });
            gameObject.SetActive(false);
            lvResetButton?.gameObject.SetActive(false);
            UpdateTabs();
        }

        public void InitializeLvReset(System.Action lvResetEvent)
        {
            lvResetButton?.gameObject.SetActive(true);
            lvResetButton?.SetCallHandler(() => 
            {
                lvResetEvent?.Invoke();
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
            magicList.SetData(skillInfoData);
            SelectCharacterTab(toggleSelect.SelectTabIndex);
        }

        public void SetAttributeList(List<ListData> list)
        {
            attributeList.SetData(list);
        }

        public void SetConditionList(List<ListData> conditionData)
        {
            conditionList.SetData(conditionData);
        }

        public void SetSkillTriggerList(List<ListData> skillTriggerLists)
        {
            skillTriggerList.SetData(skillTriggerLists);
        }

        public void SetActorInfo(ActorInfo actorInfo,List<ActorInfo> party)
        {
            actorInfoComponent.UpdateInfo(actorInfo,party);
        }

        public void SetEnemyBattlerInfo(BattlerInfo enemyInfo)
        {
            battlerInfoComponent.UpdateInfo(enemyInfo);
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
            //magicList.Activate();
            //conditionList.Deactivate();
            //skillTriggerList?.Deactivate();
        }

        public void HideActionList()
        {
            gameObject.SetActive(false);
            //magicList.Deactivate();
            //conditionList.Deactivate();
            //skillTriggerList?.Deactivate();
        }
    }

    public enum SelectCharacterTabType
    {
        Detail = 0,
        Magic = 1,
        Condition = 2,
        SkillTrigger = 3
    }
}