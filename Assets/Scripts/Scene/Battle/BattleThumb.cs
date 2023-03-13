using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleThumb : MonoBehaviour
{
    [SerializeField] private ActorInfoComponent actorInfoComponent = null;
    [SerializeField] private GameObject mainThumbRoot = null;
    [SerializeField] private GameObject awakenThumbRoot = null;

    public void ShowMainThumb(ActorInfo actorInfo)
    {
        awakenThumbRoot.SetActive(false);
        mainThumbRoot.SetActive(true);
        UpdateThumb(actorInfo);
    }

    public void ShowAwakenThumb(ActorInfo actorInfo)
    {
        mainThumbRoot.SetActive(false);
        awakenThumbRoot.SetActive(true);
        UpdateThumb(actorInfo);
    }

    public void HideThumb()
    {
        mainThumbRoot.SetActive(false);   
        awakenThumbRoot.SetActive(false);
        Clear();
    }

    private void UpdateThumb(ActorInfo actorInfo)
    {
        actorInfoComponent.UpdateInfo(actorInfo);
    }

    private void Clear()
    {
        actorInfoComponent.Clear();
    }
}
