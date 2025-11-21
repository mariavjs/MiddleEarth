using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Referências")]
    public Player player;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI distanceText;

    [Header("Ajustes - velocidade / distância")]
    public float baseSpeed = 5f;
    public float maxSpeed = 25f;
    public bool continuousIncreaseWithDistance = false;
    public float speedPerMeter = 0.01f;

    [Header("Ajustes - aumento por passos (opcional)")]
    public bool increaseSpeedWithDistance = false;
    public float speedIncreaseDistance = 100f;
    public float speedIncreaseAmount = 0.5f;
    public bool useMultiplicativeIncrease = false;
    public float speedIncreaseMultiplier = 1.05f;

    [Header("Debug / Interno")]
    [SerializeField] private float currentSpeed = 0f;
    private float elapsedTime = 0f;
    private float distance = 0f;
    private float nextSpeedIncreaseAt = 0f;

    private const string HIGH_SCORE_KEY = "HighScoreDistance";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else Destroy(gameObject);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (scene.name == "Background")
        {
            ReconnectSceneReferences();
            ResetStats();
        }
    }

    void Start()
    {
        LoadHighScore();
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;

        elapsedTime += Time.deltaTime;
        currentSpeed = baseSpeed;

        if (continuousIncreaseWithDistance)
            currentSpeed = baseSpeed + distance * speedPerMeter;

        if (currentSpeed > maxSpeed) currentSpeed = maxSpeed;

        if (increaseSpeedWithDistance && distance >= nextSpeedIncreaseAt)
        {
            ApplyStepSpeedIncrease();
            nextSpeedIncreaseAt += speedIncreaseDistance;
        }

        distance += currentSpeed * Time.deltaTime;

        if (player != null) player.speed = currentSpeed;

        UpdateUI();
    }

    void ApplyStepSpeedIncrease()
    {
        if (useMultiplicativeIncrease)
            currentSpeed *= speedIncreaseMultiplier;
        else
            currentSpeed += speedIncreaseAmount;
    }

    void UpdateUI()
    {
        if (timeText != null)
            timeText.text = "Time: " + FormatTime(elapsedTime);
        if (distanceText != null)
            distanceText.text = "Distance: " + Mathf.FloorToInt(distance) + " m";
    }

    string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        int tenths = Mathf.FloorToInt((seconds * 10f) % 10f);
        return $"{mins:00}:{secs:00}.{tenths}";
    }

    public void OnPlayerDeath()
    {
        float snapshotDistance = distance;
        float snapshotElapsed = elapsedTime;
        int snapshotSessionCoins = 0;
        int snapshotTotalCoins = 0;

        if (CoinManager.Instance != null)
        {
            snapshotSessionCoins = CoinManager.Instance.GetSessionCoins();
            snapshotTotalCoins = CoinManager.Instance.GetTotalCoins();
        }
        else
        {
            snapshotTotalCoins = PlayerPrefs.GetInt("PLAYER_COINS", 0);
            snapshotSessionCoins = PlayerPrefs.GetInt("PLAYER_COINS_SESSION", 0);
        }

        StartCoroutine(ShowGameOverSnapshotEnsureCoroutine(snapshotDistance, snapshotSessionCoins, snapshotTotalCoins, snapshotElapsed));
    }

    private IEnumerator ShowGameOverSnapshotEnsureCoroutine(float snapshotDistance, int snapshotSessionCoins, int snapshotTotalCoins, float snapshotElapsed)
    {
        SaveHighScoreIfNeeded();

        float timeout = 2f;
        float start = Time.realtimeSinceStartup;

        if (GameOverManager.Instance != null && GameOverManager.Instance.gameOverPanel == null)
        {
            GameObject foundPanel = FindGameObjectInSceneIncludingInactive("GameOverPanel") ?? FindGameObjectInSceneIncludingInactive("GameOver");
            if (foundPanel != null)
            {
                GameOverManager.Instance.gameOverPanel = foundPanel;
                Debug.Log($"[GameManager] Atribuído gameOverPanel: {foundPanel.name}");
            }
        }

        while (Time.realtimeSinceStartup - start < timeout)
        {
            if (GameOverManager.Instance != null && GameOverManager.Instance.gameOverPanel != null)
            {
                GameOverManager.Instance.ShowGameOverSnapshot(snapshotDistance, snapshotSessionCoins, snapshotTotalCoins, snapshotElapsed);
                yield break;
            }
            yield return null;
        }

        Debug.LogWarning("[GameManager] Timeout esperando GameOverManager.");
        Time.timeScale = 0f;
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

    private void LoadHighScore() => PlayerPrefs.GetFloat(HIGH_SCORE_KEY, 0f);

    void SaveHighScoreIfNeeded()
    {
        float prev = PlayerPrefs.GetFloat(HIGH_SCORE_KEY, 0f);
        if (distance > prev)
        {
            PlayerPrefs.SetFloat(HIGH_SCORE_KEY, distance);
            PlayerPrefs.Save();
        }
    }

    public float GetDistance() => distance;
    public float GetElapsedTime() => elapsedTime;
    public float GetHighScore() => PlayerPrefs.GetFloat(HIGH_SCORE_KEY, 0f);

    public void ResetStats()
    {
        elapsedTime = 0f;
        distance = 0f;
        nextSpeedIncreaseAt = speedIncreaseDistance;
        currentSpeed = baseSpeed;
        UpdateUI();
    }

    // Compatibilidade / helper para reconectar referências (em caso de builds antigos chamarem)
public void ReconnectSceneReferences()
{
    if (player == null)
    {
        player = FindObjectOfType<Player>();
        if (player != null) Debug.Log("[GameManager] Player reconectado: " + player.name);
    }

    if (timeText == null)
    {
        GameObject timeGO = GameObject.Find("Time") ?? GameObject.Find("TimeText");
        if (timeGO != null)
        {
            timeText = timeGO.GetComponent<TMPro.TextMeshProUGUI>();
            Debug.Log("[GameManager] timeText reconectado: " + timeText.name);
        }
    }

    if (distanceText == null)
    {
        GameObject distGO = GameObject.Find("Distance") ?? GameObject.Find("DistanceText");
        if (distGO != null)
        {
            distanceText = distGO.GetComponent<TMPro.TextMeshProUGUI>();
            Debug.Log("[GameManager] distanceText reconectado: " + distanceText.name);
        }
    }

    // tenta reassociar coinText no CoinManager
    if (CoinManager.Instance != null && CoinManager.Instance.coinText == null)
    {
        GameObject coinTextGO = GameObject.Find("CoinText");
        if (coinTextGO != null)
        {
            CoinManager.Instance.coinText = coinTextGO.GetComponent<TMPro.TextMeshProUGUI>();
            Debug.Log("[GameManager] coinText atribuído ao CoinManager: " + CoinManager.Instance.coinText.name);
        }
    }
}

// Compat: método usado por outros scripts
public float GetCurrentSpeed()
{
    return currentSpeed;
}

}
