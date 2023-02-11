using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEnemyLayer : MonoBehaviour
{
    [SerializeField] private List<GameObject> frontEnemyRoots;
    [SerializeField] private List<GameObject> backEnemyRoots;
    [SerializeField] private GameObject battleEnemyPrefab;
    private List<BattleEnemy> battleEnemies = new List<BattleEnemy>();

    public void Initialize(List<BattlerInfo> battlerInfos ,System.Action<BattlerInfo> callEvent)
    {
        for (int i = 0; i < frontEnemyRoots.Count;i++)
        {
            frontEnemyRoots[i].SetActive(false);
        }

        for (int i = 0; i < battlerInfos.Count;i++)
        {
            GameObject prefab = Instantiate(battleEnemyPrefab);
            frontEnemyRoots[i].SetActive(true);
            prefab.transform.SetParent(frontEnemyRoots[i].transform, false);
            var comp = prefab.GetComponent<BattleEnemy>();
            comp.SetData(battlerInfos[i]);
            comp.SetCallHandler(callEvent);
            battleEnemies.Add(comp);
        }
    }
}
