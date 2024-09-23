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
            base.SetData(listDates);
            SetBattlerInfoComp(listDates);
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
            SetSelectIndexes(indexes);
        }

        public void ClearSelect()
        {
            SetSelectIndexes(new List<int>(){-1});
            UpdateSelectIndex(-1);
            foreach (var battleBattler in _battleBattler)
            {
                battleBattler.Value.SetDisable();
            }
        }
    }
}