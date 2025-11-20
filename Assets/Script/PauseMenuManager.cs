using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Refs")]
    public GameObject pausePanel;              // painel que contém todo o popup (Canvas child)
    public Button resumeButton;
    public Button mainMenuButton;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI distanceText;

    [Header("Settings")]
    [Tooltip("Nome ou índice da cena do menu principal. Se vazio, tentará usar scene index 0.")]
    public string mainMenuSceneName = "MainMenu";
    public int mainMenuSceneIndexFallback = 0;
    [Tooltip("Se true, também pausará o áudio global via AudioListener.pause")]
    public bool pauseAudio = true;

    bool isPaused = false;

    void Start()
    {
        // garante que o painel comece fechado
        if (pausePanel != null) pausePanel.SetActive(false);

        // wiring de botões (opcional: você pode ligar no inspector)
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnBackToMainMenu);
    }

    void Update()
    {
        // atualizar as labels enquanto estiver pausado (assim o jogador vê a info atual)
        if (isPaused)
        {
            UpdateInfoTexts();
        }

        // tecla ESC para toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;

        // mostrar o painel
        if (pausePanel != null) pausePanel.SetActive(true);

        // congelar o tempo e opcionalmente o áudio
        Time.timeScale = 0f;
        if (pauseAudio) AudioListener.pause = true;

        // atualizar textos uma vez
        UpdateInfoTexts();
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;

        // esconder painel
        if (pausePanel != null) pausePanel.SetActive(false);

        // restaurar tempo/áudio
        Time.timeScale = 1f;
        if (pauseAudio) AudioListener.pause = false;
    }

    void UpdateInfoTexts()
    {
        if (timeText != null && GameManager.Instance != null)
        {
            float t = GameManager.Instance.GetElapsedTime();
            timeText.text = "Time: " + FormatTime(t);
        }

        if (distanceText != null && GameManager.Instance != null)
        {
            float d = GameManager.Instance.GetDistance();
            distanceText.text = "Distance: " + Mathf.FloorToInt(d) + " m";
        }
    }

    string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        int tenths = Mathf.FloorToInt((seconds * 10f) % 10f);
        return string.Format("{0:00}:{1:00}.{2}", mins, secs, tenths);
    }

    void OnBackToMainMenu()
    {
        // reseta timeScale antes de trocar cena
        Time.timeScale = 1f;
        if (pauseAudio) AudioListener.pause = false;

        // tenta carregar por nome; se não existir, usa fallback index
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            // Você pode preferir SceneManager.LoadScene("MainMenu");
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneIndexFallback);
        }
    }
}
