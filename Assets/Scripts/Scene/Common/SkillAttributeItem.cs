using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class SkillAttributeItem : ListItem ,IListViewItem  
    {

        [SerializeField] private Image icon;
        [SerializeField] private List<Sprite> iconSprites;

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<AttributeType>();
            
            icon.gameObject.SetActive(true);
            var spriteAtlas = iconSprites[(int)data];
            if (icon != null)
            {
                icon.sprite = spriteAtlas;
            }
        }
    }
}