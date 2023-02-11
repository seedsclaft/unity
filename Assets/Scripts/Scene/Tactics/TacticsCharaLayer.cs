using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsCharaLayer : MonoBehaviour
{
    [SerializeField] private List<GameObject> tacticsCharaRoots;
    [SerializeField] private GameObject tacticsCharaPrefab;
    private List<TacticsChara> tacticsCharas = new List<TacticsChara>();

    public void Initialize(List<ActorInfo> actorInfos ,System.Action<ActorInfo> callEvent)
    {
        for (int i = 0; i < actorInfos.Count;i++)
        {
            GameObject prefab = Instantiate(tacticsCharaPrefab);
            prefab.transform.SetParent(tacticsCharaRoots[i].transform, false);
            var comp = prefab.GetComponent<TacticsChara>();
            comp.SetData(actorInfos[i]);
            comp.SetCallHandler(callEvent);
            tacticsCharas.Add(comp);
        }
    }
}
