using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;
using UnityEngine.U2D;

public class SkillAttribute : ListItem ,IListViewItem  
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private TextMeshProUGUI learnCost;
    [SerializeField] private TextMeshProUGUI learningCountText;
    private AttributeType _data; 
    private string _valueText; 
    private int _learnCost; 
    public void SetData(AttributeType data,string valueText,int index,int learnCost = -1,string learningCount = ""){
        _data = data;
        _valueText = valueText;
        _learnCost = learnCost;
        if (learningCountText != null)
        {
            learningCountText.text = learningCount;
        }
        SetIndex(index);
    }

    public void SetData(SkillsData.SkillData.SkillAttributeInfo data,int index){
        _data = data.AttributeType;
        _valueText = data.ValueText;
        _learnCost = data.LearningCost;
        if (learningCountText != null)
        {
            learningCountText.text = data.LearningCount;
        }
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
    
    private void UpdateSkillIcon(int index)
    {
        var spriteAtlas = Resources.Load<SpriteAtlas>("Texture/Systems");
        if (icon != null)
        {
            icon.sprite = spriteAtlas.GetSprite("ElementIcon_" + (index).ToString());
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
