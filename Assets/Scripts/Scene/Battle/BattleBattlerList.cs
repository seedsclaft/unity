using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class BattleBattlerList : BaseList
    {
        [SerializeField] private List<GameObject> damageRoots;
        private Dictionary<int,BattleBattler> _battleBattler = new ();
        private int _selectIndex = -1;
        public int SelectedIndex => _selectIndex;

        public void SetData(List<ListData> listDates)
        {
            base.SetData(listDates,true,null,true);
            SetBattlerInfoComp(listDates);
        }

        public void RefreshList(List<ListData> listDates)
        {
            base.SetData(listDates);
        }

        public void SetTargetListData(List<ListData> listDates)
        {
            base.SetData(listDates);
        }

        public void SetBattlerInfoComp(List<ListData> listDates)
        {
            damageRoots.ForEach(a => a.SetActive(false));
            for (var i = 0;i < listDates.Count;i++)
            {
                var battleBattler = ItemPrefabList[i].GetComponent<BattleBattler>();
                var battlerInfo = (BattlerInfo)listDates[i].Data;
                if (battleBattler != null && battlerInfo != null)
                {
                    _battleBattler[battlerInfo.Index] = battleBattler;
                    battleBattler.SetDamageRoot(damageRoots[i]);
                }
            }
        }

        public BattlerInfoComponent GetBattlerInfoComp(int battlerIndex)
        {
            var battleBattler = _battleBattler[battlerIndex];
            if (battleBattler != null)
            {
                return battleBattler.BattlerInfoComponent;
            }
            return null;
        }
        
        public void UpdateSelectIndexList(List<int> indexes)
        {
            for (int i = 0; i < ItemPrefabList.Count;i++)
            {
                if (ItemPrefabList[i] == null) continue;
                var listItem = ItemPrefabList[i].GetComponentInChildren<ListItem>();
                if (listItem == null) continue;
                if (listItem.ListData == null) continue;
                var battler = (BattlerInfo)listItem.ListData.Data;
                if (indexes.Contains(battler.Index))
                {
                    listItem.SetSelect();
                } else
                {
                    listItem.SetUnSelect();
                }
            }
        }

        public void ClearSelect()
        {
            SetSelectIndexes(new List<int>(){-1});
            UpdateSelectIndex(-1);
            /*
            foreach (var battleBattler in _battleBattler)
            {
                battleBattler.Value.SetDisable();
            }
            */
        }
    }
}