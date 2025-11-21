using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Referências")]
    public Player player;                  // arraste seu Player aqui (opcional)
    public TextMeshProUGUI timeText;       // arraste TimeText (TMP)
    public TextMeshProUGUI distanceText;   // arraste DistanceText (TMP)

    [Header("Ajustes - velocidade / distância")]
    public float baseSpeed = 5f;                 // velocidade inicial do mundo
    public float maxSpeed = 25f;                 // teto da velocidade
    [Tooltip("Se true, a velocidade cresce continuamente com a distância (linear).")]
    public bool continuousIncreaseWithDistance = false;
    [Tooltip("Quanto a velocidade cresce por unidade de distância (apenas para continuousIncreaseWithDistance).")]
    public float speedPerMeter = 0.01f;

    [Header("Ajustes - aumento por passos (opcional)")]
    public bool increaseSpeedWithDistance = false;   // comportamento legacy (se quer passos)
    public float speedIncreaseDistance = 100f;       // a cada X metros aplica aumento por passo
    public float speedIncreaseAmount = 0.5f;         // valor adicionado ao currentSpeed por passo (aditivo)
    public bool useMultiplicativeIncrease = false;   // se true, multiplica por speedIncreaseMultiplier
    public float speedIncreaseMultiplier = 1.05f;    // multiplicador por passo

    [Header("Debug / Interno")]
    [SerializeField] private float currentSpeed = 0f;   // velocidade atual do mundo (exposta só pra debug)
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
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // chamado toda vez que uma cena é carregada
private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    // garante que o jogo não fique pausado por herança
    Time.timeScale = 1f;
    AudioListener.pause = false;

    // Se a sua cena de jogo se chama "Background" (como você disse), só reconecta lá
    if (scene.name == "Background")
    {
        ReconnectSceneReferences();
        ResetStats();
    }

    // debug rápido
    PrintSceneDebugStatus();
}

private void ReconnectSceneReferences()
{
    // tenta encontrar o Player na nova cena
    if (player == null)
    {
        player = FindObjectOfType<Player>();
        if (player != null) Debug.Log("[GameManager] Player encontrado: " + player.name);
    }

    // tenta associar TimeText (tenta por nome - ajuste se o objeto tiver outro nome)
    if (timeText == null)
    {
        GameObject timeGO = GameObject.Find("Time"); // ou "TimeText"
        if (timeGO == null) timeGO = GameObject.Find("TimeText");
        if (timeGO != null)
        {
            timeText = timeGO.GetComponent<TextMeshProUGUI>();
            Debug.Log("[GameManager] timeText reconectado: " + timeText.name);
        }
        else Debug.LogWarning("[GameManager] Não encontrou TimeText por nome.");
    }

    // distancia
    if (distanceText == null)
    {
        GameObject distGO = GameObject.Find("Distance"); // ou "DistanceText"
        if (distGO == null) distGO = GameObject.Find("DistanceText");
        if (distGO != null)
        {
            distanceText = distGO.GetComponent<TextMeshProUGUI>();
            Debug.Log("[GameManager] distanceText reconectado: " + distanceText.name);
        }
        else Debug.LogWarning("[GameManager] Não encontrou DistanceText por nome.");
    }

    // CoinManager: tenta encontrar a instância na cena atual
    if (CoinManager.Instance == null)
    {
        CoinManager cm = FindObjectOfType<CoinManager>();
        if (cm != null)
        {
            Debug.Log("[GameManager] CoinManager encontrado dinamicamente.");
            // a instancia do CoinManager será atribuída pelo próprio Awake dele
        }
        else
        {
            Debug.LogWarning("[GameManager] CoinManager NÃO encontrado na cena!");
        }
    }

    // Reatribui coinText no CoinManager caso esteja null e exista Text no Canvas
    if (CoinManager.Instance != null && CoinManager.Instance.coinText == null)
    {
        // tenta localizar por nome (ajuste o nome do objeto de texto conforme sua hierarquia)
        GameObject coinTextGO = GameObject.Find("CoinsPanel")?.transform.Find("CoinText")?.gameObject;
        if (coinTextGO == null)
        {
            coinTextGO = GameObject.Find("CoinText");
        }
        if (coinTextGO != null)
        {
            var tmp = coinTextGO.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                CoinManager.Instance.coinText = tmp;
                Debug.Log("[GameManager] atribuído coinText ao CoinManager: " + tmp.name);
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] coinText GameObject não encontrado (procure pelo nome correto).");
        }
    }

    // garante que sessionCoins seja resetado no CoinManager quando nova partida começar
    if (CoinManager.Instance != null)
    {
        CoinManager.Instance.ResetSessionCoins();
    }
}


    void Start()
    {
        // carregamento do highscore apenas
        LoadHighScore();

        // se a cena já estiver carregada antes do registro, chamamos a rotina de limpeza manual
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;

        // tempo
        elapsedTime += Time.deltaTime;

        // --- calcula a velocidade atual do mundo (currentSpeed) ---
        // base
        currentSpeed = baseSpeed;

        // se aumento contínuo com a distância estiver ligado:
        if (continuousIncreaseWithDistance)
        {
            currentSpeed = baseSpeed + distance * speedPerMeter;
        }

        // limita pelo teto
        if (currentSpeed > maxSpeed) currentSpeed = maxSpeed;

        // se ainda usar aumento por passos (legacy/alternativa), checa se atingimos o próximo checkpoint
        if (increaseSpeedWithDistance && distance >= nextSpeedIncreaseAt)
        {
            ApplyStepSpeedIncrease();
            nextSpeedIncreaseAt += speedIncreaseDistance;
        }

        // garanta o teto após step increase
        if (currentSpeed > maxSpeed) currentSpeed = maxSpeed;

        // --- atualiza a distância com base na velocidade do mundo (o mundo move-se currentSpeed unidades/s) ---
        distance += currentSpeed * Time.deltaTime * 1f; // distanceMultiplier se necessário pode ser aplicado aqui

        // --- aplica a velocidade calculada ao player (single source of truth) ---
        if (player != null)
        {
            player.speed = currentSpeed;
        }

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
        if (timeText != null) timeText.text = "Time: " + FormatTime(elapsedTime);
        if (distanceText != null) distanceText.text = "Distance: " + Mathf.FloorToInt(distance) + " m";
    }

    string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        int tenths = Mathf.FloorToInt((seconds * 10f) % 10f);
        return string.Format("{0:00}:{1:00}.{2}", mins, secs, tenths);
    }

    // chamado pelo Player quando morrer (opcional)
public void OnPlayerDeath()
{
    SaveHighScoreIfNeeded();
    StartCoroutine(ShowGameOverDelayed());
}

private IEnumerator ShowGameOverDelayed()
{
    yield return new WaitForSecondsRealtime(0.2f);

    // tenta reobter o GameOverManager se o Instance sumiu
    if (GameOverManager.Instance == null)
    {
        var manager = FindObjectOfType<GameOverManager>();
        if (manager != null)
        {
            Debug.Log("[GameManager] GameOverManager encontrado dinamicamente.");
            manager.ShowGameOver();
            yield break;
        }
    }

    if (GameOverManager.Instance != null)
        GameOverManager.Instance.ShowGameOver();
    else
        Debug.LogWarning("[GameManager] GameOverManager ainda não encontrado após a morte do jogador.");
}


    private void LoadHighScore()
    {
        // garante que a chave exista
        float prev = PlayerPrefs.GetFloat(HIGH_SCORE_KEY, 0f);
    }

    void SaveHighScoreIfNeeded()
    {
        float prev = PlayerPrefs.GetFloat(HIGH_SCORE_KEY, 0f);
        if (distance > prev)
        {
            PlayerPrefs.SetFloat(HIGH_SCORE_KEY, distance);
            PlayerPrefs.Save();
            Debug.Log("[GameManager] Novo recorde: " + Mathf.FloorToInt(distance) + " m");
        }
    }

    public float GetDistance() => distance;
    public float GetElapsedTime() => elapsedTime;
    public float GetHighScore() => PlayerPrefs.GetFloat(HIGH_SCORE_KEY, 0f);
    public float GetCurrentSpeed() => currentSpeed;

    public void ResetStats()
    {
        elapsedTime = 0f;
        distance = 0f;
        nextSpeedIncreaseAt = speedIncreaseDistance;
        currentSpeed = baseSpeed;
        UpdateUI();
    }

    // exemplo: restart via botão
    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PrintSceneDebugStatus()
{
    Debug.Log("[DEBUG] --- Scene debug status ---");
    Debug.Log("[DEBUG] Scene name: " + SceneManager.GetActiveScene().name);

    Debug.Log("[DEBUG] GameManager.player is " + (player == null ? "NULL" : player.name));
    Debug.Log("[DEBUG] GameManager.timeText is " + (timeText == null ? "NULL" : timeText.name));
    Debug.Log("[DEBUG] GameManager.distanceText is " + (distanceText == null ? "NULL" : distanceText.name));

    if (CoinManager.Instance == null)
    {
        Debug.Log("[DEBUG] CoinManager.Instance is NULL");
    }
    else
    {
        Debug.Log("[DEBUG] CoinManager.Instance exists. SessionCoins: " + CoinManager.Instance.GetSessionCoins() + " TotalCoins: " + CoinManager.Instance.GetTotalCoins());
        Debug.Log("[DEBUG] CoinManager.coinText is " + (CoinManager.Instance.coinText == null ? "NULL" : CoinManager.Instance.coinText.name));
    }

    Debug.Log("[DEBUG] GameManager distance: " + GetDistance() + " elapsedTime: " + GetElapsedTime());
    Debug.Log("[DEBUG] --- end debug ---");
}
}
