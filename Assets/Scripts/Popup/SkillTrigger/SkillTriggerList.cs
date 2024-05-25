using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class SkillTriggerList : BaseList
    {
        [SerializeField] private SkillTriggerHelp skillTriggerHelp = null;
        private int _selectItemIndex = -1;
        public new void Initialize()
        {
            base.Initialize();
            SetSelectedHandler(() => SelectSkillTriggerList());
        }

        private void SelectSkillTriggerList()
        {
            var listData = ListData;
            if (listData != null)
            {
                var data = (SkillTriggerInfo)listData.Data;
                skillTriggerHelp.UpdateSkillInfo(data.SkillInfo);
                skillTriggerHelp.UpdateSkillTriggerHelp(data.SkillTriggerDates[0]?.Help,data.SkillTriggerDates?[1].Help);
            }
        }

        public void SetInputCallHandler()
        {
            //SetInputCallHandler((a) => CallInputHandler(a));
            SetInputHandler(InputKeyType.Right,() => CallInputHandler(InputKeyType.Right));
            SetInputHandler(InputKeyType.Left,() => CallInputHandler(InputKeyType.Left));
            SetInputHandler(InputKeyType.Down,() => CallInputHandler(InputKeyType.Down));
            SetInputHandler(InputKeyType.Up,() => CallInputHandler(InputKeyType.Up));
        }

        public void SetInputListHandler(Action skillEvent,Action trigger1Event,Action trigger2Event,Action upButtonEvent,Action downButtonEvent)
        {
            for (int i = 0; i < ItemPrefabList.Count;i++)
            {
                var skillTriggerListItem = ItemPrefabList[i].GetComponent<SkillTriggerListItem>();
                skillTriggerListItem.SetInputListHandler(skillEvent,trigger1Event,trigger2Event,upButtonEvent,downButtonEvent);
                skillTriggerListItem.SetSelectItemHandler(
                    (a) => 
                    {
                        _selectItemIndex = 0;
                        UpdateListItem(a);
                    },
                    (a) => 
                    {
                        _selectItemIndex = 1;
                        UpdateListItem(a);
                    },
                    (a) => 
                    {
                        _selectItemIndex = 2;
                        UpdateListItem(a);
                    }
                );
            }
        }

        
        private void CallInputHandler(InputKeyType keyType)
        {
            if (keyType == InputKeyType.Right)
            {
                if (_selectItemIndex < 2)
                {
                    _selectItemIndex++;
                }
                UpdateListItem(Index);
                return;
            }
            if (keyType == InputKeyType.Left)
            {
                if (_selectItemIndex <= 0)
                {
                    _selectItemIndex--;
                }
                UpdateListItem(Index);
                return;
            }
            UpdateListItem(Index);
        }

        private void UpdateListItem(int selectIndex)
        {
            if (_selectItemIndex < 0) return;
            var startIndex = GetStartIndex();
            var skillTriggerListItem = ItemPrefabList[selectIndex - startIndex].GetComponent<SkillTriggerListItem>();
            skillTriggerListItem.UpdateItemIndex(_selectItemIndex);
            foreach (var ItemPrefab in ItemPrefabList)
            {
                if (ItemPrefab != ItemPrefabList[selectIndex - startIndex])
                {
                    var skillTriggerListItem1 = ItemPrefab.GetComponent<SkillTriggerListItem>();
                    skillTriggerListItem1.UpdateItemIndex(-1);
                }
            }
        }
    }
}
