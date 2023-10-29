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

    public void SetData(SkillData.SkillAttributeInfo data,int index){
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
        if (icon != null)
        {
            icon.gameObject.SetActive(_data != AttributeType.None);
            UpdateElementIcon(Index);
        }
        
        if (valueText != null && _valueText != null)
        {
            valueText.gameObject.SetActive(_data != AttributeType.None);
            valueText.text = _valueText;
        }
        if (learnCost != null && _learnCost != -1)
        {
            learnCost.gameObject.SetActive(_data != AttributeType.None);
            learnCost.text = _learnCost.ToString();
        }
        if (allAttributeText != null)
        {
            allAttributeText.gameObject.SetActive(_data == AttributeType.None);
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
        allAttributeText.color = new Color(1,1,1,1);
    }

    public new void SetUnSelect()
    {
        base.SetUnSelect();
        icon.color = new Color(1,1,1,0.5f);
        allAttributeText.color = new Color(1,1,1,0.5f);
    }
}
