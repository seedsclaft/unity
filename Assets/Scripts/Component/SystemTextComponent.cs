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
            var textData = DataSystem.System.GetTextData(id);
            if (textData == null)
            {
                Debug.Log("error" + gameObject.name);
            }
            var textMeshPro = gameObject.GetComponent<TextMeshProUGUI>();
            if (textMeshPro != null)
            {
                textMeshPro.text = textData.Text;
            }
        }
    }
}
