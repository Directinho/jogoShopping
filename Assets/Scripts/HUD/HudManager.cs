using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    [Header("Botões do HUD")]
    public Button cursorButton; // Atribua o UI-Cursor no Inspector
    public Button moveButton; // Atribua o UI-Move no Inspector
    public Button trashButton; // Atribua o UI-Trash no Inspector

    void Start()
    {
        if (cursorButton != null)
        {
            cursorButton.onClick.AddListener(() => MouseManager.Instance.SetMode(MouseManager.MouseMode.Place));
        }

        if (moveButton != null)
        {
            moveButton.onClick.AddListener(() => MouseManager.Instance.SetMode(MouseManager.MouseMode.Drag));
        }

        if (trashButton != null)
        {
            trashButton.onClick.AddListener(() => MouseManager.Instance.SetMode(MouseManager.MouseMode.Delete));
        }
    }
}