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
            var turnWaits = new Dictionary<BattlerInfo,float>();
            _battlerInfos.Sort((a,b) => a.Ap < b.Ap ? -1 : 1);
            foreach (var battler in _battlerInfos)
            {
                if (battler.IsAlive())
                {
                    turnWaits[battler] = battler.WaitFrame(0);
                }
            }

            var idx = 0;
            foreach (var turnWait in turnWaits)
            {
                //if (i > 6) continue;
                var battler = turnWait.Key;
                _actorBattlers[idx].UpdateAlpha(battler.IsActor);
                _enemyBattlers[idx].UpdateAlpha(!battler.IsActor);
                if (battler.IsActor)
                {
                    _actorBattlers[idx].UpdateInfo(battler,(int)turnWait.Value,idx);
                } else
                {
                    _enemyBattlers[idx].UpdateInfo(battler,(int)turnWait.Value,idx);
                }
                idx++;
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