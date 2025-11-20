using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    public PauseMenuManager pauseManager;
    public Button button;

    void Start()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null && pauseManager != null)
        {
            button.onClick.AddListener(() => pauseManager.PauseGame());
        }
    }
}
