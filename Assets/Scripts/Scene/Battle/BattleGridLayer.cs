using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class BattleGridLayer : MonoBehaviour
    {
        [SerializeField] private GameObject battleGridPrefab;
        [SerializeField] private GameObject battleGridEnemyPrefab;
        [SerializeField] private GameObject actorRoot;
        [SerializeField] private GameObject enemyRoot;
        private Dictionary<BattlerInfo,BattlerInfoComponent> _battlers = new ();
        private List<BattlerGrid> _actorBattlers = new ();
        private List<BattlerGrid> _enemyBattlers = new ();
        private List<BattlerInfo> _battlerInfos = new ();

        public void Initialize()
        {
            for (int i = 0; i < 10;i++)
            {
                var prefab = Instantiate(battleGridPrefab);
                prefab.transform.SetParent(actorRoot.transform, false);
                var comp = prefab.GetComponent<BattlerGrid>();
                comp.UpdateAlpha(false);
                _actorBattlers.Add(comp);
            }
            for (int i = 0; i < 10;i++)
            {
                var prefab = Instantiate(battleGridEnemyPrefab);
                prefab.transform.SetParent(enemyRoot.transform, false);
                var comp = prefab.GetComponent<BattlerGrid>();
                comp.UpdateAlpha(false);
                _enemyBattlers.Add(comp);
            }
        }

        public void SetActorInfo(List<BattlerInfo> battlerInfos)
        {
            for (int i = 0; i < battlerInfos.Count;i++)
            {
                _battlerInfos.Add(battlerInfos[i]);
            }
        }
        
        public void SetEnemyInfo(List<BattlerInfo> battlerInfos)
        {
            for (int i = 0; i < battlerInfos.Count;i++)
            {
                _battlerInfos.Add(battlerInfos[i]);
            }
        }

        public void UpdatePosition()
        {
            var waitFrameList = new List<float>();
            var turnWait = new Dictionary<BattlerInfo,List<float>>();
            foreach (var battler in _battlerInfos)
            {
                if (battler.IsAlive())
                {
                    turnWait[battler] = new List<float>();
                    for (int i = 0;i < 7;i++)
                    {
                        var waitFrame = battler.WaitFrame(i);
                        if (!waitFrameList.Contains(waitFrame))
                        {
                            waitFrameList.Add(waitFrame);
                        }
                        turnWait[battler].Add(waitFrame);
                    }
                }
            }
            waitFrameList.Sort((a,b) => a < b ? -1 : 1);
            var sortedBattlerList = new List<BattlerInfo>();
            var sortedBattlerApList = new List<float>();
            var targetIndex = 0;
            while (sortedBattlerList.Count < 7)
            {
                var ap = waitFrameList[targetIndex];
                targetIndex++;
                foreach (var turnW in turnWait)
                {
                    var findIndex = turnWait[turnW.Key].FindIndex(a => a == ap);
                    if (findIndex > -1)
                    {
                        sortedBattlerList.Add(turnW.Key);
                        sortedBattlerApList.Add(ap);
                    }
                }
            }

            for (int i = 0;i < sortedBattlerList.Count;i++)
            {
                if (i > 6) continue;
                var battler = sortedBattlerList[i];
                _actorBattlers[i].UpdateAlpha(battler.IsActor);
                _enemyBattlers[i].UpdateAlpha(!battler.IsActor);
                if (battler.IsActor)
                {
                    _actorBattlers[i].UpdateInfo(battler,(int)sortedBattlerApList[i],i);
                } else
                {
                    _enemyBattlers[i].UpdateInfo(battler,(int)sortedBattlerApList[i],i);
                }
            }
        }

        public void RefreshStatus()
        {
            foreach (var data in _battlers)
            {
                data.Value.RefreshStatus();
                data.Value.gameObject.SetActive(data.Key.IsAlive());
            }
        }
    }
}