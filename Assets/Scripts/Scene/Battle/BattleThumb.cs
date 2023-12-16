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
        gameObject.SetActive(false);
        var awaken = battlerInfo.IsState(StateType.Demigod);
        var image = awaken ? actorInfoComponent.AwakenFaceThumb : actorInfoComponent.MainThumb;
        BaseAnimation.MoveAndFade(gameObject.GetComponent<RectTransform>(),image,-24,0,0);
        BaseAnimation.MoveAndFade(gameObject.GetComponent<RectTransform>(),image,0,1,0.1f);
        awakenThumbRoot.SetActive(awaken);
        mainThumbRoot.SetActive(!awaken);
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

    public void ShowAllThumb(ActorInfo actorInfo)
    {
        awakenThumbRoot.SetActive(true);
        mainThumbRoot.SetActive(true);
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
