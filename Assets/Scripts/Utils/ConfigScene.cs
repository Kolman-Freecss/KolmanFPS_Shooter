#if UNITY_EDITOR

#region

using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Utils
{
    [ExecuteInEditMode]
    public class ConfigScene : MonoBehaviour
    {
        public TMP_FontAsset newFont;
        public Sprite backgroundButton;

        [Header("Buttons")] [Tooltip("Font size for buttons")] [SerializeField]
        float buttonFontSize = 24f;

        [Header("Texts")] [Tooltip("Font size for texts")] [SerializeField]
        float textFontSize = 20f;

        [SerializeField] Color textColor = Color.black;

        [ContextMenu("Change Fonts")]
        void ChangeFonts()
        {
            TextMeshProUGUI[] textObjects = FindObjectsOfType<TextMeshProUGUI>();

            foreach (TextMeshProUGUI textObject in textObjects)
            {
                Undo.RecordObject(textObject, "Changed Font");
                textObject.font = newFont;
                textObject.color = textColor;
                textObject.horizontalAlignment = HorizontalAlignmentOptions.Center;
                textObject.verticalAlignment = VerticalAlignmentOptions.Middle;
                textObject.fontSize = textFontSize;
                textObject.enableAutoSizing = false;
            }
        }

        [ContextMenu("Change Buttons")]
        void ChangeButtons()
        {
            Button[] buttons = FindObjectsOfType<Button>();

            foreach (Button button in buttons)
            {
                Undo.RecordObject(button, "Changed Button");
                Image image = button.GetComponent<Image>();
                if (backgroundButton)
                {
                    image.sprite = backgroundButton;
                }

                image.color = Color.white; // #4B4B4B en formato RGB
                button.transition = Selectable.Transition.ColorTint;
                // Make transition highlighted color the same as normal color
                ColorBlock colors = button.colors;
                colors.highlightedColor = new Color32(255, 0, 0, 255); // #FF0000 en formato RGB;
                button.colors = colors;
                button.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
                button.GetComponentInChildren<TextMeshProUGUI>().fontSize = buttonFontSize;
            }
        }

        [ContextMenu("Change Toogles")]
        void ChangeToogles()
        {
            Toggle[] toggles = FindObjectsOfType<Toggle>();

            foreach (Toggle toggle in toggles)
            {
                Undo.RecordObject(toggle, "Changed Toggle");
                ColorBlock colors = toggle.colors;
                colors.normalColor = new Color32(236, 183, 183, 255); // #ECB7B7 en formato RGB
                colors.highlightedColor = new Color32(255, 0, 0, 255); // #FF0000 en formato RGB;
                toggle.colors = colors;
            }
        }

        [ContextMenu("Change Sliders")]
        void ChangeSliders()
        {
            Slider[] sliders = FindObjectsOfType<Slider>();

            foreach (Slider slider in sliders)
            {
                Undo.RecordObject(slider, "Changed Slider");
                RectTransform rectTransform = slider.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(500, 30);


                // Get Fill Area image
                Image fillArea = slider.transform.GetChild(1).GetChild(0).GetComponent<Image>();
                fillArea.color = new Color32(195, 99, 99, 255); // #C36363 en formato RGB
            }
        }
    }
}
#endif