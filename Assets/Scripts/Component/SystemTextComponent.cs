using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SystemTextComponent : MonoBehaviour
{
    [SerializeField] private int id;

    public void Awake(){
        if (id != 0)
        {
            TextData textData = DataSystem.System.GetTextData(id);
            if (textData == null)
            {
                Debug.Log("error" + gameObject.name);
            }
            TextMeshProUGUI textMeshPro = gameObject.GetComponent<TextMeshProUGUI>();
            if (textMeshPro != null)
            {
                textMeshPro.text = textData.Text;
            }
        }
    }
}
