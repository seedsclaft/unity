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
        var awaken = false;//battlerInfo.IsAwaken;
        var image = awaken ? actorInfoComponent.AwakenThumb : actorInfoComponent.MainThumb;
        gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-24,0,0);
        image.color = new Color(255,255,255,0);
        BaseAnimation.MoveAndFade(gameObject.GetComponent<RectTransform>(),image,0,1,0.1f);
        awakenThumbRoot.SetActive(awaken);
        mainThumbRoot.SetActive(!awaken);
        gameObject.SetActive(true);
        UpdateThumb(battlerInfo.ActorInfo.Master);
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
