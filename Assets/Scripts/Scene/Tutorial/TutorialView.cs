using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialView : BaseView
{
    [SerializeField] private Image focusImage = null;
    [SerializeField] private Image focusBgImage = null;
    [SerializeField] private Image arrowImage = null;

    public void ShowFocusImage(float x,float y,float width,float height)
    {
        Debug.Log("ShowFocusImage");
        gameObject.SetActive(true);
        if (focusImage == null) return;
        var rect = focusImage.GetComponent<RectTransform>();
        rect.localPosition = new Vector3(x,y,0);
        rect.sizeDelta = new Vector3(width,height);
        var bgRect = focusBgImage.GetComponent<RectTransform>();
        bgRect.localPosition = new Vector3(x * -1,y * -1,0);
    }

    public void HideFocusImage()
    {
        gameObject.SetActive(false);
    }
}
