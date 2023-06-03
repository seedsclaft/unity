using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGridLayer : MonoBehaviour
{
    [SerializeField] private GameObject battleGridprefab;
    [SerializeField] private GameObject actorRoot;
    [SerializeField] private GameObject enemyRoot;
    private Dictionary<BattlerInfo,BattlerInfoComponent> _data = new Dictionary<BattlerInfo,BattlerInfoComponent>();
    public void SetActorInfo(List<BattlerInfo> battlerInfos)
    {
        for (int i = 0; i < battlerInfos.Count;i++)
        {
            GameObject prefab = Instantiate(battleGridprefab);
            prefab.transform.SetParent(actorRoot.transform, false);
            var comp = prefab.GetComponent<BattlerInfoComponent>();
            comp.UpdateInfo(battlerInfos[i]);
            _data[battlerInfos[i]] = comp;
            prefab.GetComponentInChildren<EnemyInfoComponent>().Clear();
        }
        UpdatePosition();
        RefreshStatus();
    }
    
    public void SetEnemyInfo(List<BattlerInfo> battlerInfos)
    {
        for (int i = 0; i < battlerInfos.Count;i++)
        {
            GameObject prefab = Instantiate(battleGridprefab);
            prefab.transform.SetParent(enemyRoot.transform, false);
            var comp = prefab.GetComponent<BattlerInfoComponent>();
            comp.UpdateInfo(battlerInfos[i]);
            int gridKey = 0;
            foreach (var item in _data)
            {
                if (item.Key.EnemyData != null)
                {
                    if (item.Key.EnemyData.Id == battlerInfos[i].EnemyData.Id)
                    {
                        gridKey++;
                    }
                }
            }
            comp.SetEnemyGridKey(gridKey);
            _data[battlerInfos[i]] = comp;
            prefab.GetComponentInChildren<ActorInfoComponent>().Clear();
        }
        UpdatePosition();
        RefreshStatus();
    }

    public void UpdatePosition()
    {
        List<BattlerInfo> battlerInfos = new List<BattlerInfo>();
        foreach (var data in _data)
        {
            RectTransform rect = data.Value.gameObject.GetComponent < RectTransform > ();
            rect.localPosition = new Vector3(rect.localPosition.x, data.Key.Ap, 0);
            battlerInfos.Add(data.Key);
        }
        battlerInfos.Sort((a,b)=> a.Ap - b.Ap);
        foreach (var info in battlerInfos)
        {
            _data[info].gameObject.transform.SetAsFirstSibling();
        }
        /*
        foreach (var data in _data)
        {
            int index = battlerInfos.FindIndex(a => a.Index == data.Key.Index);
            data.Value.gameObject.transform.SetSiblingIndex(index * 10);
        }
        */
    }

    public void RefreshStatus()
    {
        foreach (var data in _data)
        {
            data.Value.RefreshStatus();
            data.Value.gameObject.SetActive(data.Key.IsAlive());
        }
    }
}
