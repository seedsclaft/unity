using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;
using Effekseer;
using DG.Tweening;

public class BattleEnemy : ListItem 
{
    [SerializeField] private BattlerInfoComponent battlerInfoComponent;
    public BattlerInfoComponent BattlerInfoComponent {get {return battlerInfoComponent;}}
    [SerializeField] private Image enemyImage;
    [SerializeField] private EffekseerEmitter effekseerEmitter;
    [SerializeField] private GameObject statusObject;

    private List<BattleDamage> _battleDamages = new List<BattleDamage>();

    private BattlerInfo _battlerInfo;
    private string textureName = null;

    public int EnemyIndex{
        get {return _battlerInfo.Index;}
    }

    public void SetData(BattlerInfo battlerInfo,int index)
    {
        battlerInfoComponent.UpdateInfo(battlerInfo);
        _battlerInfo = battlerInfo;
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
    
    public new void SetSelectHandler(System.Action<int> handler){
		ContentEnterListener enterListener = clickButton.gameObject.AddComponent<ContentEnterListener> ();
        enterListener.SetEnterEvent(() => handler(_battlerInfo.Index));
    }

    private void Update() {
        if (enemyImage.mainTexture != null && textureName != enemyImage.mainTexture.name)
        {
            UpdateSizeDelta();
            textureName = enemyImage.mainTexture.name;
        }
    }

    private void UpdateSizeDelta()
    {
        int width = Mathf.Max(240,enemyImage.mainTexture.width);
        int height = Mathf.Max(240,enemyImage.mainTexture.height);
        RectTransform objectRect = gameObject.GetComponent < RectTransform > ();
        RectTransform rect = Cursor.GetComponent < RectTransform > ();
        RectTransform effectRect = effekseerEmitter.gameObject.GetComponent < RectTransform > ();
        rect.sizeDelta = new Vector2(width,height);
        objectRect.sizeDelta = new Vector2(width,height);
        effectRect.sizeDelta = new Vector2(width,height);
    }
}
