using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SystemTextComponent : MonoBehaviour
{
    [SerializeField] private int id;

    public void Awake(){
        if (id != null)
        {
            TextData textData = DataSystem.System.SystemTextData.Find(a => a.Id == id);
            TextMeshProUGUI textMeshPro = gameObject.GetComponent<TextMeshProUGUI>();
            if (textMeshPro != null)
            {
                textMeshPro.text = textData.Text;
            }
        }
    }
}
