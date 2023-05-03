using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class SkillAttribute : ListItem ,IListViewItem  
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private TextMeshProUGUI learnCost;
    private AttributeType _data; 
    private string _valueText; 
    private int _learnCost; 
    public void SetData(AttributeType data,string valueText,int index,int learnCost = -1){
        _data = data;
        _valueText = valueText;
        _learnCost = learnCost;
        SetIndex(index);
    }

    public void SetCallHandler(System.Action<AttributeType> handler)
    {
        clickButton.onClick.AddListener(() => handler((AttributeType)_data));
    }

    public void UpdateViewItem()
    {
        if (_data == AttributeType.None) return;
        if (icon != null)
        {
            UpdateSkillIcon(Index);
        }
        
        if (valueText != null && _valueText != null)
        {
            valueText.text = _valueText;
        }
        if (learnCost != null && _learnCost != -1)
        {
            learnCost.text = _learnCost.ToString();
        }
    }
    
    private async void UpdateSkillIcon(int index)
    {
        var handle = await ResourceSystem.LoadAsset<IList<Sprite>>("Assets/Images/System/ElementIcon.png");
        if (icon != null)
        {
            icon.sprite = handle[index];
        }
    }

    public new void SetSelect()
    {
        icon.color = new Color(1,1,1,1);
    }

    public new void SetUnSelect()
    {
        base.SetUnSelect();
        icon.color = new Color(1,1,1,0.5f);
    }
}
