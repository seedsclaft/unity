using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleThumb : MonoBehaviour
{
    [SerializeField] private ActorInfoComponent actorInfoComponent = null;
    [SerializeField] private GameObject mainThumbRoot = null;
    [SerializeField] private GameObject awakenThumbRoot = null;

    public void ShowMainThumb(ActorData actorData)
    {
        awakenThumbRoot.SetActive(false);
        mainThumbRoot.SetActive(true);
        UpdateThumb(actorData);
    }

    public void ShowAwakenThumb(ActorData actorData)
    {
        mainThumbRoot.SetActive(false);
        awakenThumbRoot.SetActive(true);
        UpdateThumb(actorData);
    }

    public void HideThumb()
    {
        mainThumbRoot.SetActive(false);   
        awakenThumbRoot.SetActive(false);
        Clear();
    }

    private void UpdateThumb(ActorData actorData)
    {
        actorInfoComponent.UpdateData(actorData);
    }

    private void Clear()
    {
        actorInfoComponent.Clear();
    }
}
