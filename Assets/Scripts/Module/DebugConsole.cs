using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DebugConsole : MonoBehaviour
{
    [SerializeField] private TMP_InputField consoleInputField = null;
    void Start()
    {
#if UNITY_EDITOR
        consoleInputField.onEndEdit.AddListener((a) => CallConsoleCommand(a));
#else
        gameObject.SetActive(false);
#endif
    }

    void CallConsoleCommand(string inputText)
    {
        if (consoleInputField.text == "R")
        {
            SceneManager.LoadScene(0);
        }
        if (consoleInputField.text == "S1")
        {
            SaveSystem.SaveStart(GameSystem.CurrentData,1);
        }
        if (consoleInputField.text == "S2")
        {
            SaveSystem.SaveStart(GameSystem.CurrentData,2);
        }
        if (consoleInputField.text == "S3")
        {
            SaveSystem.SaveStart(GameSystem.CurrentData,3);
        }
        if (consoleInputField.text == "L1")
        {
            SaveSystem.LoadStart(1);
        }
        if (consoleInputField.text == "L2")
        {
            SaveSystem.LoadStart(2);
        }
        if (consoleInputField.text == "L3")
        {
            SaveSystem.LoadStart(3);
        }
    }
}
