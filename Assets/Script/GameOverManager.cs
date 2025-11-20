using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("Refs - UI")]
    public GameObject gameOverPanel;            // painel root (desativa por padrão)
    public TextMeshProUGUI scoreText;           // "Score: 123 m"
    public TextMeshProUGUI recordText;          // "Record: 456 m"
    public TextMeshProUGUI coinsThisRunText;    // "Coins: 10"
    public TextMeshProUGUI coinsTotalText;      // "Total Coins: 100"
    public Button mainMenuButton;               // botão voltar ao menu principal
    public Button shopButton;                   // botão ir ao shop
    public Button restartButton;                // (opcional) botão reiniciar partida

    [Header("Scene names / indices")]
    public string mainMenuSceneName = "MainMenu";
    public string shopSceneName = "Shop";
    public int mainMenuSceneIndexFallback = 0;
    public int shopSceneIndexFallback = 2;

    // runtime
    private int coinsThisRun = 0;
    private float finalDistance = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        if (shopButton != null) shopButton.onClick.AddListener(OnShop);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null && gameOverPanel.activeSelf) return;

        // pausa o jogo / áudio
        Time.timeScale = 0f;
        AudioListener.pause = true;

        // distância final (pega do GameManager)
        finalDistance = (GameManager.Instance != null) ? GameManager.Instance.GetDistance() : 0f;

        // moedas desta sessão: usar CoinManager diretamente quando disponível
        if (CoinManager.Instance != null)
        {
            coinsThisRun = CoinManager.Instance.GetSessionCoins();
        }
        else
        {
            // fallback: checar PlayerPrefs (se você gravou sessão)
            coinsThisRun = PlayerPrefs.GetInt("SessionCoins", 0);
        }

        // total de moedas (leitura apenas)
        int totalCoins = (CoinManager.Instance != null) ? CoinManager.Instance.GetTotalCoins() : PlayerPrefs.GetInt("PLAYER_COINS", 0);

        // exibir score/record
        if (scoreText != null) scoreText.text = "Score: " + Mathf.FloorToInt(finalDistance) + " m";
        if (recordText != null)
        {
            float record = (GameManager.Instance != null) ? GameManager.Instance.GetHighScore() : PlayerPrefs.GetFloat("HighScoreDistance", 0f);
            recordText.text = "Record: " + Mathf.FloorToInt(record) + " m";
        }

        if (coinsThisRunText != null) coinsThisRunText.text = "Coins: " + coinsThisRun;
        if (coinsTotalText != null) coinsTotalText.text = "Total Coins: " + totalCoins;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void OnMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (!string.IsNullOrEmpty(mainMenuSceneName)) SceneManager.LoadScene(mainMenuSceneName);
        else SceneManager.LoadScene(mainMenuSceneIndexFallback);
    }

    public void OnShop()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (!string.IsNullOrEmpty(shopSceneName)) SceneManager.LoadScene(shopSceneName);
        else SceneManager.LoadScene(shopSceneIndexFallback);
    }

    public void OnRestart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
