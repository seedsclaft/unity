using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleThumb : MonoBehaviour
{
    [SerializeField] private ActorInfoComponent actorInfoComponent = null;
    [SerializeField] private BattlerInfoComponent battlerInfoComponent = null;
    [SerializeField] private GameObject mainThumbRoot = null;
    [SerializeField] private GameObject awakenThumbRoot = null;

    public void ShowMainThumb(BattlerInfo battlerInfo,ActorData actorData)
    {
        awakenThumbRoot.SetActive(false);
        mainThumbRoot.SetActive(true);
        gameObject.SetActive(true);
        UpdateStatus(battlerInfo);
        UpdateThumb(actorData);
    }

    public void ShowAwakenThumb(BattlerInfo battlerInfo,ActorData actorData)
    {
        mainThumbRoot.SetActive(false);
        awakenThumbRoot.SetActive(true);
        gameObject.SetActive(true);
        UpdateStatus(battlerInfo);
        UpdateThumb(actorData);
    }

    public void HideThumb()
    {
        mainThumbRoot.SetActive(false);   
        awakenThumbRoot.SetActive(false);
        gameObject.SetActive(false);
        Clear();
    }

    private void UpdateThumb(ActorData actorData)
    {
        actorInfoComponent.UpdateData(actorData);
    }    

    private void UpdateStatus(BattlerInfo battlerInfo)
    {
        battlerInfoComponent.UpdateInfo(battlerInfo);
        battlerInfoComponent.RefreshStatus();
    }

    private void Clear()
    {
        actorInfoComponent.Clear();
    }
}
