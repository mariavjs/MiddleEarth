using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

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
    // private int coinsThisRun = 0;
    // private float finalDistance = 0f;

    // fields (áudio)
    public AudioClip deathClip;             // arraste o clip de morte no inspector
    public AudioSource sfxSource;           // opcional: arraste um AudioSource (em GameOverManager) ou deixe null

    // controla até quando o SFX deve tocar (tempo real)
    private float sfxEndRealtime = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        HideGameOver(); // garante que comece invisível

    }

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        if (shopButton != null) shopButton.onClick.AddListener(OnShop);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
    }

// substitua a implementação atual de ShowGameOver() por esta
public void ShowGameOver()
{
    if (gameOverPanel != null && gameOverPanel.activeSelf) return;

    // --- atualizar valores antes de pausar para garantir que rodem dependências ---
    UpdateGameOverTexts();

    // pausa o jogo (física / updates dependentes de Time.timeScale)
    Time.timeScale = 0f;

    // Tocar o SFX de morte e registrar quando ele termina (em tempo real)
    if (deathClip != null)
    {
        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(deathClip);
        }
        else
        {
            Vector3 pos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(deathClip, pos);
        }

        sfxEndRealtime = Time.realtimeSinceStartup + deathClip.length;
    }
    else
    {
        sfxEndRealtime = Time.realtimeSinceStartup;
    }

    // finalmente mostra o painel
    if (gameOverPanel != null) gameOverPanel.SetActive(true);
}

private void UpdateGameOverTexts()
{
    // DISTANCE / SCORE
    float runDistance = 0f;
    float best = 0f;
    if (GameManager.Instance != null)
    {
        runDistance = GameManager.Instance.GetDistance();
        best = GameManager.Instance.GetHighScore();
    }
    // formata: "Run Score: 123 m" e "Max Score: 456 m"
    if (scoreText != null)
        scoreText.text = "Run Score: " + Mathf.FloorToInt(runDistance) + " m";
    if (recordText != null)
        recordText.text = "Max Score: " + Mathf.FloorToInt(best) + " m";

    // COINS: tenta CoinManager então PlayerPrefs fallback
    int sessionCoins = 0;
    int totalCoins = 0;
    if (CoinManager.Instance != null)
    {
        sessionCoins = CoinManager.Instance.GetSessionCoins();
        totalCoins = CoinManager.Instance.GetTotalCoins();
    }
    else
    {
        // PlayerPrefs fallback (mantive mesma chave que usa CoinManager)
        sessionCoins = PlayerPrefs.GetInt("PLAYER_COINS_SESSION", 0); // se você não usa essa chave, ignore
        totalCoins = PlayerPrefs.GetInt("PLAYER_COINS", 0);
    }

    if (coinsThisRunText != null)
        coinsThisRunText.text = "+Coins: " + sessionCoins;
    if (coinsTotalText != null)
        coinsTotalText.text = "Total Coins: " + totalCoins;

    // (opcional) se você tiver outros campos no painel (runCoins / totalCoins) atualize-os também:
    // <procure pelos nomes exatos na sua hierarchy e adicione referências públicas no script, se necessário>
}


    public void HideGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }


    // verifica se ainda estamos no período em que o SFX deve tocar
    bool IsSfxStillPlayingRealtime()
    {
        return Time.realtimeSinceStartup < sfxEndRealtime - 0.0001f;
    }

    // Carregamento de cena: aguarda o SFX terminar antes de trocar
    IEnumerator LoadSceneAfterSfx_Coroutine(string sceneName, int fallbackIndex)
    {
        // espera até que o SFX termine (em tempo real)
        while (IsSfxStillPlayingRealtime())
        {
            yield return null; // continua checando em tempo real (Time.timeScale == 0 não afeta)
        }

        // restaura tempo e áudio global antes de trocar de cena
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Faz o carregamento da cena
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            SceneManager.LoadScene(fallbackIndex);
    }

    public void OnMainMenu()
    {
        Debug.Log("[GameOverManager] OnMainMenu called");
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu"); // ou pelo índice 2
    }

    public void OnShop()
    {
        SceneManager.LoadScene("Shop"); // ou pelo índice 2
    }

    public void OnRestart()
    {
        // Restart também espera o SFX terminar
        StartCoroutine(RestartAfterSfxCoroutine());
    }

    IEnumerator RestartAfterSfxCoroutine()
    {
        while (IsSfxStillPlayingRealtime())
        {
            yield return null;
        }

        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
