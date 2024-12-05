using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PanelController : MonoBehaviour
{
    // Reference to the Canvas that this script controls.
    [SerializeField]
    private GameObject canvasToControl; // Change the type to GameObject

    // Hides the associated canvas when the game starts.
    public void Start()
    {
        HideCanvas();
    }

    // Shows the controlled canvas.
    public void ShowCanvas()
    {
        // Simple null check to avoid NullReferenceException.
        if (canvasToControl != null)
        {
            canvasToControl.SetActive(true);
        }
        else
        {
            Debug.LogWarning("canvasToControl is not assigned.");
        }
    }

    // Hides the controlled canvas.
   public void HideCanvas()
    {
        if (canvasToControl != null)
        {
            canvasToControl.SetActive(false);
        }
        else
        {
            Debug.LogWarning("canvasToControl is not assigned.");
        }
    }
}