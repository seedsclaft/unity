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
    [SerializeField] private TextMeshProUGUI allAttributeText;
    private string _valueText; 
    private int _learnCost; 

    public void SetData(SkillData.SkillAttributeInfo data,int index){
        //_data = data.AttributeType;
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
        //clickButton.onClick.AddListener(() => handler((AttributeType)_data));
    }

    public void UpdateViewItem()
    {
        if (ListData == null) return;
        var data = (SkillData.SkillAttributeInfo)ListData.Data;
        _valueText = data.ValueText;
        _learnCost = data.LearningCost;
        if (learningCountText != null)
        {
            learningCountText.text = data.LearningCount;
        }
        if (icon != null)
        {
            icon.gameObject.SetActive(data.AttributeType != AttributeType.None);
            UpdateElementIcon((int)data.AttributeType - 1);
        }
        
        if (valueText != null && _valueText != null)
        {
            valueText.gameObject.SetActive(data.AttributeType != AttributeType.None);
            valueText.text = _valueText;
        }
        if (learnCost != null && _learnCost != -1)
        {
            learnCost.gameObject.SetActive(data.AttributeType != AttributeType.None);
            learnCost.text = _learnCost.ToString();
        }
        if (allAttributeText != null)
        {
            allAttributeText.gameObject.SetActive(data.AttributeType == AttributeType.None);
        }
        if (ListData.Selected)
        {
            SetSelect();
        } else
        {
            SetUnSelect();
        }
    }
    
    private void UpdateElementIcon(int index)
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
        if (allAttributeText) allAttributeText.color = new Color(1,1,1,1);
    }

    public new void SetUnSelect()
    {
        base.SetUnSelect();
        icon.color = new Color(1,1,1,0.5f);
        if (allAttributeText) allAttributeText.color = new Color(1,1,1,0.5f);
    }
}
