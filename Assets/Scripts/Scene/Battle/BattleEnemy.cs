using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Effekseer;

public class BattleEnemy : ListItem 
{
    [SerializeField] private BattlerInfoComponent battlerInfoComponent;
    public BattlerInfoComponent BattlerInfoComponent {get {return battlerInfoComponent;}}
    [SerializeField] private Image enemyImage;
    [SerializeField] private EffekseerEmitter effekseerEmitter;
    [SerializeField] private GameObject statusObject;
    [SerializeField] private EffekseerEmitter cursorEffekseerEmitter;

    private BattlerInfo _battlerInfo;
    private string textureName = null;
    private bool _isFront = false;

    public int EnemyIndex{
        get {return _battlerInfo.Index;}
    }

    public void SetData(BattlerInfo battlerInfo,int index,bool isFront)
    {
        _battlerInfo = battlerInfo;
        _isFront = isFront;
        battlerInfoComponent.UpdateInfo(battlerInfo);
        SetIndex(index);
    }
    
    public void SetDamageRoot(GameObject damageRoot)
    {
        battlerInfoComponent.SetDamageRoot(damageRoot);
    }

    public void SetStatusRoot(GameObject statusRoot)
    {
        statusObject.transform.SetParent(statusRoot.transform, false);
        statusRoot.SetActive(true);
        battlerInfoComponent.SetStatusRoot(statusObject);
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        if (_battlerInfo == null) return;
        clickButton.onClick.AddListener(() => handler(_battlerInfo.Index));
    }
    
    public new void SetSelectHandler(System.Action<int> handler)
    {
		ContentEnterListener enterListener = clickButton.gameObject.AddComponent<ContentEnterListener> ();
        enterListener.SetEnterEvent(() => handler(_battlerInfo.Index));
    }

    public void SetPressHandler(System.Action<int> handler)
    {
		ContentPressListener pressListener = clickButton.gameObject.AddComponent<ContentPressListener> ();
        pressListener.SetPressEvent(() => handler(_battlerInfo.Index));
    }

    private void Update() 
    {
        if (enemyImage.mainTexture != null && textureName != enemyImage.mainTexture.name)
        {
            UpdateSizeDelta();
            textureName = enemyImage.mainTexture.name;
        }
    }

    private void UpdateSizeDelta()
    {
        int size = _isFront == true ? 200 : 240;
        int width = Mathf.Max(size,enemyImage.mainTexture.width);
        int height = Mathf.Max(size,enemyImage.mainTexture.height);
        RectTransform objectRect = gameObject.GetComponent < RectTransform > ();
        RectTransform rect = Cursor.GetComponent < RectTransform > ();
        RectTransform effectRect = effekseerEmitter.gameObject.GetComponent < RectTransform > ();
        RectTransform statusRect = statusObject.GetComponent < RectTransform > ();
        RectTransform damageRect = battlerInfoComponent.BattleDamageRoot.gameObject.GetComponent < RectTransform > ();
        rect.sizeDelta = new Vector2(width,height);
        objectRect.sizeDelta = new Vector2(width,height);
        effectRect.sizeDelta = new Vector2(width,height);
        if (_isFront == false && height > 394)
        {
            statusRect.sizeDelta = new Vector2(0,height/2);
            damageRect.sizeDelta = new Vector2(0,height/2);
        }
    }

    public void EnableClick()
    {
        clickButton.enabled = true;
    }

    public void DisableClick()
    {
        clickButton.enabled = false;
    }
    
    public void SetSelect(EffekseerEffectAsset effekseerEffectAsset)
    {
        if (Cursor == null) return;
        Cursor.SetActive(true);
        cursorEffekseerEmitter.Stop();
        cursorEffekseerEmitter.Play(effekseerEffectAsset);
        cursorEffekseerEmitter.enabled = true;
        cursorEffekseerEmitter.playOnStart = true;
        cursorEffekseerEmitter.isLooping = true;
    }

    public new void SetUnSelect()
    {
        if (Cursor == null) return;
        Cursor.SetActive(false);
        cursorEffekseerEmitter.enabled = false;
        cursorEffekseerEmitter.playOnStart = false;
        cursorEffekseerEmitter.isLooping = false;
    }
}
