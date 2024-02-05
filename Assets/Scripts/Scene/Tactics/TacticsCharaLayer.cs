using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsCharaLayer : MonoBehaviour
{
    [SerializeField] private List<GameObject> tacticsCharaRoots;
    [SerializeField] private GameObject tacticsCharaPrefab;

    private List<TacticsChara> _tacticsCharacters = new List<TacticsChara>();

    public void SetData(List<ActorInfo> actorInfos)
    {
        _tacticsCharacters.ForEach(a => a.gameObject.SetActive(false));
        for (int i = 0; i < actorInfos.Count;i++)
        {
            if (tacticsCharaRoots.Count > i)
            {
                if (_tacticsCharacters.Count <= i)
                {
                    var prefab = Instantiate(tacticsCharaPrefab);
                    prefab.transform.SetParent(tacticsCharaRoots[i].transform, false);
                    var comp = prefab.GetComponent<TacticsChara>();
                    _tacticsCharacters.Add(comp);
                }
                var rectTransform = tacticsCharaRoots[i].GetComponent<RectTransform>();
                _tacticsCharacters[i].gameObject.SetActive(true);
                _tacticsCharacters[i].Initialize(gameObject,rectTransform.localPosition.x,rectTransform.localPosition.y,rectTransform.localScale.x);
                _tacticsCharacters[i].SetData(actorInfos[i]);
            }
        }
    }
}
