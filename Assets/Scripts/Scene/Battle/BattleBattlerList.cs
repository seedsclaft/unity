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

        public new void SetData(List<ListData> listDates)
        {
            base.SetData(listDates);
            SetBattlerInfoComp();
        }

        public void SetTargetListData(List<ListData> listDates)
        {
            base.SetData(listDates);
        }

        public void SetBattlerInfoComp()
        {
            damageRoots.ForEach(a => a.SetActive(false));
            for (var i = 0;i < ItemPrefabList.Count;i++)
            {
                var battleBattler = ItemPrefabList[i].GetComponent<BattleBattler>();
                if (battleBattler.ListData != null)
                {
                    var battlerInfo = (BattlerInfo)battleBattler.ListData.Data;
                    if (battleBattler != null && battlerInfo != null)
                    {
                        _battleBattler[battlerInfo.Index] = battleBattler;
                        battleBattler.SetDamageRoot(damageRoots[i]);
                    }
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
        
        public void UpdateSelectIndexList(List<int> indexes){
            SetSelectIndexes(indexes);
        }

        /*
        private void CallInputHandler(InputKeyType keyType, System.Action<List<int>> callEvent)
        {
            if (keyType == InputKeyType.Decide)
            {
                var battlerInfo = _battleInfos.Find(a => a.Index == _selectIndex);
                if (battlerInfo == null)
                {
                    return;
                }
                if (battlerInfo.IsAlive() == false)
                {
                    return;
                }
                if (_targetIndexList.IndexOf(_selectIndex) == -1)
                {
                    return;
                }
                callEvent(MakeTargetIndexes(battlerInfo));
            }
            if (keyType == InputKeyType.Right)
            {
                var current = _battleInfos.Find(a => a.Index == _selectIndex);
                if (current != null)
                {
                    var target = _battleInfos.Find(a => a.Index > current.Index && a.IsAlive());
                    if (target != null)
                    {
                        if (current.LineIndex == target.LineIndex)
                        {
                            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                            UpdateBattlerIndex(target.Index);
                        }
                    }
                }
            }
            if (keyType == InputKeyType.Left)
            {
                var current = _battleInfos.Find(a => a.Index == _selectIndex);
                if (current != null)
                {
                    var targets = _battleInfos.FindAll(a => a.Index < current.Index && a.IsAlive() && current.LineIndex == a.LineIndex);
                    if (targets.Count > 0)
                    {
                        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                        UpdateBattlerIndex(targets[targets.Count-1].Index);
                    }
                }
            }
            if (keyType == InputKeyType.Up)
            {
                var current = _battleInfos.Find(a => a.Index == _selectIndex);
                if (current != null)
                {
                    var target = _battleInfos.Find(a => a.Index > current.Index && a.IsAlive() && a.LineIndex == LineType.Back);
                    if (target != null)
                    {
                        if (current != target)
                        {
                            UpdateBattlerIndex(target.Index);
                        }
                    }
                }
            }
            if (keyType == InputKeyType.Down)
            {
                var current = _battleInfos.Find(a => a.Index == _selectIndex);
                if (current != null)
                {
                    var target = _battleInfos.Find(a => a.Index < current.Index && a.IsAlive() && a.LineIndex == LineType.Front);
                    if (target != null)
                    {
                        if (current != target)
                        {
                            UpdateBattlerIndex(target.Index);
                        }
                    }
                }
            }
        }
        */

    /*
        private List<int> MakeTargetIndexes(BattlerInfo battlerInfo)
        {
            var indexList = new List<int>();
            if (_targetScopeType == ScopeType.All || _targetScopeType == ScopeType.WithoutSelfAll)
            {
                for (int i = 0; i < _battleBattler.Count;i++)
                {
                    if (_battleInfos[i].IsAlive())
                    {
                        indexList.Add(_battleInfos[i].Index);
                    }
                }
            } else
            if (_targetScopeType == ScopeType.Line || _targetScopeType == ScopeType.FrontLine)
            {
                for (int i = 0; i < _battleBattler.Count;i++)
                {
                    if (battlerInfo.LineIndex == _battleInfos[i].LineIndex)
                    {
                        if (_battleInfos[i].IsAlive())
                        {
                            indexList.Add(_battleInfos[i].Index);
                        }
                    }
                }
            } else
            if (_targetScopeType == ScopeType.One || _targetScopeType == ScopeType.WithoutSelfOne)
            {
                if (battlerInfo.IsAlive())
                {
                    indexList.Add(battlerInfo.Index);
                }
            } else
            if (_targetScopeType == ScopeType.Self)
            {
                if (battlerInfo.IsAlive())
                {
                    indexList.Add(battlerInfo.Index);
                }
            }
            return indexList;
        }
        */

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