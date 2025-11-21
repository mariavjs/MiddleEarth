using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("UI Refs")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI recordText;
    public TextMeshProUGUI coinsThisRunText;
    public TextMeshProUGUI coinsTotalText;
    public Button mainMenuButton;
    public Button shopButton;
    public Button restartButton;

    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";
    public string shopSceneName = "Shop";

    [Header("Audio")]
    public AudioClip deathClip;
    public AudioSource sfxSource;
    private float sfxEndRealtime = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameOverPanel == null)
        {
            GameObject found = FindGameObjectInSceneIncludingInactive("GameOverPanel") ?? FindGameObjectInSceneIncludingInactive("GameOver");
            if (found != null)
            {
                gameOverPanel = found;
                Debug.Log($"[GameOverManager] gameOverPanel auto-atribuído: {found.name}");
            }
        }

        HideGameOver();
    }

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        if (shopButton != null) shopButton.onClick.AddListener(OnShop);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
    }

    public void ShowGameOverSnapshot(float runDistance, int sessionCoins, int totalCoins, float elapsedTime)
    {
        if (scoreText != null)
            scoreText.text = $"Run Score: {Mathf.FloorToInt(runDistance)} m";

        if (recordText != null)
        {
            float highScore = PlayerPrefs.GetFloat("HighScoreDistance", 0f);
            if (GameManager.Instance != null)
                highScore = GameManager.Instance.GetHighScore();
            recordText.text = "Max Score: " + Mathf.FloorToInt(highScore) + " m";
        }

        if (coinsThisRunText != null)
            coinsThisRunText.text = $"+Coins: {sessionCoins}";

        if (coinsTotalText != null)
            coinsTotalText.text = $"Total Coins: {totalCoins}";

        Time.timeScale = 0f;

        if (deathClip != null)
        {
            if (sfxSource != null)
                sfxSource.PlayOneShot(deathClip);
            else
                AudioSource.PlayClipAtPoint(deathClip, Camera.main?.transform.position ?? Vector3.zero);

            sfxEndRealtime = Time.realtimeSinceStartup + deathClip.length;
        }
        else sfxEndRealtime = Time.realtimeSinceStartup;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        else Debug.LogWarning("[GameOverManager] gameOverPanel é NULL.");
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void OnMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnShop()
    {
        SceneManager.LoadScene(shopSceneName);
    }

    public void OnRestart()
    {
        StartCoroutine(RestartAfterSfxCoroutine());
    }

    IEnumerator RestartAfterSfxCoroutine()
    {
        while (Time.realtimeSinceStartup < sfxEndRealtime)
            yield return null;

        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    GameObject FindGameObjectInSceneIncludingInactive(string name)
    {
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in roots)
        {
            var t = RecursiveFind(root.transform, name);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    Transform RecursiveFind(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            var found = RecursiveFind(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    // Compat: método legacy que outros scripts podem chamar
// Ele usa a implementação atual por baixo: atualiza os textos e usa o snapshot flow atual.
public void ShowGameOver()
{
    // se já há painel ativo, nada a fazer
    if (gameOverPanel != null && gameOverPanel.activeSelf) return;

    // atualiza os textos lendo as fontes (GameManager/CoinManager)
    UpdateGameOverTexts();

    // pausa, toca sfx e mostra (mesma lógica do snapshot)
    Time.timeScale = 0f;

    if (deathClip != null)
    {
        if (sfxSource != null) sfxSource.PlayOneShot(deathClip);
        else AudioSource.PlayClipAtPoint(deathClip, Camera.main != null ? Camera.main.transform.position : Vector3.zero);

        sfxEndRealtime = Time.realtimeSinceStartup + deathClip.length;
    }
    else
    {
        sfxEndRealtime = Time.realtimeSinceStartup;
    }

    if (gameOverPanel != null) gameOverPanel.SetActive(true);
    else Debug.LogWarning("[GameOverManager] ShowGameOver chamado mas gameOverPanel é NULL.");
}

// Atualiza textos lendo diretamente das fontes (compatibilidade)
private void UpdateGameOverTexts()
{
    float runDistance = 0f;
    float best = 0f;
    if (GameManager.Instance != null)
    {
        runDistance = GameManager.Instance.GetDistance();
        best = GameManager.Instance.GetHighScore();
    }

    if (scoreText != null) scoreText.text = "Run Score: " + Mathf.FloorToInt(runDistance) + " m";
    if (recordText != null) recordText.text = "Max Score: " + Mathf.FloorToInt(best) + " m";

    int sessionCoins = 0;
    int totalCoins = 0;
    if (CoinManager.Instance != null)
    {
        sessionCoins = CoinManager.Instance.GetSessionCoins();
        totalCoins = CoinManager.Instance.GetTotalCoins();
    }
    else
    {
        sessionCoins = PlayerPrefs.GetInt("PLAYER_COINS_SESSION", 0);
        totalCoins = PlayerPrefs.GetInt("PLAYER_COINS", 0);
    }

    if (coinsThisRunText != null) coinsThisRunText.text = "+Coins: " + sessionCoins;
    if (coinsTotalText != null) coinsTotalText.text = "Total Coins: " + totalCoins;
}

}
