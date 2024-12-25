using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class BattleGridLayer : MonoBehaviour
    {
        [SerializeField] private GameObject actorPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject actorRoot;
        [SerializeField] private GameObject enemyRoot;
        private Dictionary<BattlerInfo,BattlerGrid> _battlers = new ();

        public void SetGridMembers(List<BattlerInfo> battlerInfos)
        {
            SetActorInfo(battlerInfos.FindAll(a => a.IsActor));
            SetEnemyInfo(battlerInfos.FindAll(a => !a.IsActor));
        }

        private void SetActorInfo(List<BattlerInfo> battlerInfos)
        {
            for (int i = 0; i < battlerInfos.Count;i++)
            {
                GameObject prefab = Instantiate(actorPrefab);
                prefab.transform.SetParent(actorRoot.transform, false);
                var comp = prefab.GetComponent<BattlerGrid>();
                comp.UpdateInfo(battlerInfos[i]);
                _battlers[battlerInfos[i]] = comp;
            }
            UpdatePosition();
            RefreshStatus();
        }
        
        private void SetEnemyInfo(List<BattlerInfo> battlerInfos)
        {
            for (int i = 0; i < battlerInfos.Count;i++)
            {
                GameObject prefab = Instantiate(enemyPrefab);
                prefab.transform.SetParent(enemyRoot.transform, false);
                var comp = prefab.GetComponent<BattlerGrid>();
                comp.UpdateInfo(battlerInfos[i]);
                int gridKey = 0;
                foreach (var item in _battlers)
                {
                    if (item.Key.EnemyData != null)
                    {
                        if (item.Key.EnemyData.Id == battlerInfos[i].EnemyData.Id)
                        {
                            gridKey++;
                        }
                    }
                }
                _battlers[battlerInfos[i]] = comp;
                //comp.SetGridKey(gridKey);
            }
            UpdatePosition();
            RefreshStatus();
        }

        public void UpdatePosition()
        {
            var battlerInfos = new List<BattlerInfo>();
            foreach (var data in _battlers)
            {
                var rect = data.Value.gameObject.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(rect.localPosition.x, data.Key.Ap, 0);
                battlerInfos.Add(data.Key);
            }
            battlerInfos.Sort((a,b)=> (int)a.Ap - (int)b.Ap);
            foreach (var info in battlerInfos)
            {
                _battlers[info].gameObject.transform.SetAsFirstSibling();
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