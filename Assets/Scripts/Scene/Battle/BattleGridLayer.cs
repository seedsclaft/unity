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
        }
        UpdatePosition();
    }
    
    public void SetEnemyInfo(List<BattlerInfo> battlerInfos)
    {
        for (int i = 0; i < battlerInfos.Count;i++)
        {
            GameObject prefab = Instantiate(battleGridprefab);
            prefab.transform.SetParent(enemyRoot.transform, false);
            var comp = prefab.GetComponent<BattlerInfoComponent>();
            comp.UpdateInfo(battlerInfos[i]);
            _data[battlerInfos[i]] = comp;
        }
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        foreach (var data in _data)
        {
            RectTransform rect = data.Value.gameObject.GetComponent < RectTransform > ();
            rect.localPosition = new Vector3(rect.localPosition.x, data.Key.Ap, 0);
        }
    }
}
