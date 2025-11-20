using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        ResetStats();
        LoadHighScore();
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
        // abrir painel de Game Over, etc.
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
}
