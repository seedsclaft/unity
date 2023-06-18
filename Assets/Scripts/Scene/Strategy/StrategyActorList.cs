using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrategyActorList : MonoBehaviour
{   
    [SerializeField] private int rows = 0;
    [SerializeField] private GameObject actorPrefab = null;
    [SerializeField] private List<GameObject> actorRoots = null;

    private List<ActorInfo> _data = new List<ActorInfo>();

    private List<StrategyActor> _comps = new List<StrategyActor>();
    public void Initialize()
    {
        for (int i = 0; i < rows;i++)
        {
            var prefab = Instantiate(actorPrefab);
            prefab.transform.SetParent(actorRoots[i].transform, false);
            _comps.Add(prefab.GetComponent<StrategyActor>());
        }
    }

    public void StartResultAnimation(List<ActorInfo> actors,List<bool> isBonusList,System.Action callEvent)
    {
        for (int i = 0; i < _comps.Count;i++)
        {
            bool isBonus = (isBonusList != null && isBonusList.Count > i) ? isBonusList[i] : false;
            _comps[i].gameObject.SetActive(false);
            if (i < actors.Count)
            {
                _comps[i].SetData(actors[i],isBonus);
                _comps[i].StartResultAnimation(i);
                _comps[i].gameObject.SetActive(true);
                if (i == actors.Count-1)
                {
                    _comps[i].SetEndCallEvent(callEvent);
                }
            }
        }
    }
}
