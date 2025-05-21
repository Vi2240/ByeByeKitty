using UnityEngine;
using UnityEngine.UIElements;

public class ToggleUIDocument : MonoBehaviour
{
    // The single UI document that will be toggled using both Tab and I keys
    public UIDocument uiDocument;

    // Keeps track of whether the UI is currently visible
    private bool isUIVisible = false;

    void Start()
    {
        // Hide the UI at the start of the game
        SetUIDocumentVisible(uiDocument, false);
    }

    void Update()
    {
        // Toggle the UI when either Tab or I key is pressed
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
        {
            isUIVisible = !isUIVisible;
            SetUIDocumentVisible(uiDocument, isUIVisible);
        }
    }

    // Helper method to show or hide the UI document
    void SetUIDocumentVisible(UIDocument uiDoc, bool visible)
    {
        if (uiDoc != null && uiDoc.rootVisualElement != null)
        {
            uiDoc.rootVisualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
