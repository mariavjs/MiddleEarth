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
    private int coinsThisRun = 0;
    private float finalDistance = 0f;

    // fields (áudio)
    public AudioClip deathClip;             // arraste o clip de morte no inspector
    public AudioSource sfxSource;           // opcional: arraste um AudioSource (em GameOverManager) ou deixe null

    // controla até quando o SFX deve tocar (tempo real)
    private float sfxEndRealtime = 0f;

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
                // PlayClipAtPoint também funciona; vamos registrar o tempo fim com base no length.
                Vector3 pos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
                AudioSource.PlayClipAtPoint(deathClip, pos);
            }

            // registra o instante (realtime) em que o SFX termina
            sfxEndRealtime = Time.realtimeSinceStartup + deathClip.length;
        }
        else
        {
            // não há clip: garante que sfxEndRealtime não bloqueie carregamento
            sfxEndRealtime = Time.realtimeSinceStartup;
        }

        // atualizar UI e mostrar painel (mantemos o áudio tocando)
        // restante do seu código (atualização de textos de score/coins deveria estar aqui, se não estiver já)
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
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
        StartCoroutine(LoadSceneAfterSfx_Coroutine(mainMenuSceneName, mainMenuSceneIndexFallback));
    }

    public void OnShop()
    {
        StartCoroutine(LoadSceneAfterSfx_Coroutine(shopSceneName, shopSceneIndexFallback));
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
