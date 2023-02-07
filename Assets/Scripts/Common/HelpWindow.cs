using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HelpWindow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI helpText;

    public void SetHelpText(string text){
        helpText.text = text;
    }
}
