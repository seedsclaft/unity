using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using TMPro;

namespace Ryneus
{
    public class InputInfoComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI guideText;
        [SerializeField] private Image guideIcon;
        [SerializeField] private List<Sprite> keyboardIcons;
        [SerializeField] private List<Sprite> gamePadIcons;

        public void SetData(SystemData.InputData inputData)
        {
            if (guideText != null && inputData.Name != "\"\"")
            {
                guideText.text = inputData.Name;
                var sizeDelta = guideText.GetComponent<RectTransform>().sizeDelta;
                guideText.GetComponent<RectTransform>().sizeDelta = new Vector2(guideText.preferredWidth,sizeDelta.y);
            }
            if (inputData.Name == "\"\"")
            {
                guideText.text = "";
                var sizeDelta = guideText.GetComponent<RectTransform>().sizeDelta;
                int space = -16;
                if (InputSystem.IsGamePad){
                    space = -4;
                }
                guideText.GetComponent<RectTransform>().sizeDelta = new Vector2(space,sizeDelta.y);
            }
            if (guideIcon != null)
            {
                UpdateGuideIcon(inputData.KeyId);
            }
        }

        private void UpdateGuideIcon(int keyId)
        {
            if (InputSystem.IsGamePad)
            {
                guideIcon.sprite = gamePadIcons[keyId];
                var wide = false;
                var width = wide == true ? 42 : 28;
                var height = wide == true ? 48 : 28;
                var sizeDelta = guideIcon.gameObject.GetComponent<RectTransform>().sizeDelta;
                guideIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(width,height);
            } else
            {
                guideIcon.sprite = keyboardIcons[keyId];
                var wide = ((keyId+1) == (int)InputKeyType.Decide || (keyId+1) == (int)InputKeyType.Cancel);
                var width = wide == true ? 42 : 28;
                var height = wide == true ? 48 : 28;
                var sizeDelta = guideIcon.gameObject.GetComponent<RectTransform>().sizeDelta;
                guideIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(width,height);
            }
        }
    }
}