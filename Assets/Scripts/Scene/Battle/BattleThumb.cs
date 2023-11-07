using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleThumb : MonoBehaviour
{
    [SerializeField] private ActorInfoComponent actorInfoComponent = null;
    [SerializeField] private GameObject mainThumbRoot = null;
    [SerializeField] private GameObject awakenThumbRoot = null;


    public void ShowBattleThumb(BattlerInfo battlerInfo)
    {
        awakenThumbRoot.SetActive(battlerInfo.IsState(StateType.Demigod));
        mainThumbRoot.SetActive(!battlerInfo.IsState(StateType.Demigod));
        gameObject.SetActive(true);
        UpdateThumb(battlerInfo.ActorInfo.Master);
    }

    public void ShowActorThumb(ActorInfo actorInfo,bool isAwake)
    {
        awakenThumbRoot.SetActive(isAwake);
        mainThumbRoot.SetActive(isAwake == false);
        gameObject.SetActive(true);
        UpdateThumb(actorInfo.Master);
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

    private void Clear()
    {
        actorInfoComponent.Clear();
    }
}
