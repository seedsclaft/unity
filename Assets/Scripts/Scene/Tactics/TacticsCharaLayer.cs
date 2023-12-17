using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsCharaLayer : MonoBehaviour
{
    [SerializeField] private List<GameObject> tacticsCharaRoots;
    [SerializeField] private GameObject tacticsCharaPrefab;

    public void SetData(List<ActorInfo> actorInfos)
    {
        for (int i = 0; i < actorInfos.Count;i++)
        {
            var prefab = Instantiate(tacticsCharaPrefab);
            prefab.transform.SetParent(tacticsCharaRoots[i].transform, false);
            var comp = prefab.GetComponent<TacticsChara>();
            var rectTransform = tacticsCharaRoots[i].GetComponent<RectTransform>();
            comp.Initialize(gameObject,rectTransform.localPosition.x,rectTransform.localPosition.y,rectTransform.localScale.x);
            comp.SetData(actorInfos[i]);
        }
    }
}
