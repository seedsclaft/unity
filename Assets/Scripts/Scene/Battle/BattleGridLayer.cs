using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGridLayer : MonoBehaviour
{
    [SerializeField] private GameObject battleGridprefab;
    [SerializeField] private GameObject actorRoot;
    [SerializeField] private GameObject enemyRoot;
    private List<BattlerInfoComponent> _data = new List<BattlerInfoComponent>();

    public void SetActorInfo(List<BattlerInfo> battlerInfos)
    {
        for (int i = 0; i < battlerInfos.Count;i++)
        {
            GameObject prefab = Instantiate(battleGridprefab);
            prefab.transform.SetParent(actorRoot.transform, false);
            var comp = prefab.GetComponent<BattlerInfoComponent>();
            comp.UpdateInfo(battlerInfos[i]);
            _data.Add(comp);
        }
    }
}
