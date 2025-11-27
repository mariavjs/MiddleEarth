using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Tilemaps;

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
    public float speedIncreaseMultiplier = 1.05f; // multiplicador por passo
    public float hellSpeedMultiplier = 1.5f;

    [Header("Debug / Interno")]
    [SerializeField] private float currentSpeed = 0f;   // velocidade atual do mundo (exposta só pra debug)
    private float elapsedTime = 0f;
    private float distance = 0f;
    private float nextSpeedIncreaseAt = 0f;

    [Header("Tilemaps")]
    [SerializeField]  private TilemapController groundTilemap;
    [SerializeField]  private TilemapController hellTilemap;

    private const string HIGH_SCORE_KEY = "HighScoreDistance";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PitManager.Instance.OnPlayerGoToHell += OnPlayerGoToHell;
        PitManager.Instance.OnPlayerBackFromHell += OnPlayerBackFromHell;

        groundTilemap.SetIsMoving(true);
        hellTilemap.SetIsMoving(false);

        // carregamento do highscore apenas
        LoadHighScore();

        // se a cena já estiver carregada antes do registro, chamamos a rotina de limpeza manual
        Time.timeScale = 1f;
        AudioListener.pause = false;

        groundTilemap.SetIsMoving(true);

        // tenta resolver referências que mudam por cena
        if (player == null)
            player = FindObjectOfType<Player>();

        // garante que o GameOver esteja fechado
        if (GameOverManager.Instance != null)
            GameOverManager.Instance.HideGameOver();

        // reseta stats somente quando entramos na cena de jogo (ajuste o nome/índice conforme seu projeto)
        if (SceneManager.GetActiveScene().buildIndex == 1)
            ResetStats();
    }

    private void OnPlayerGoToHell()
    {
        TilemapController[] tiles = FindObjectsByType<TilemapController>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);

        foreach (TilemapController tilemap in tiles)
        {
            tilemap.SetIsMoving(tilemap.tilemapType != TilemapType.GROUND);
        }
    }

    private void OnPlayerBackFromHell()
    {
        TilemapController[] tiles = FindObjectsByType<TilemapController>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);

        foreach (TilemapController tilemap in tiles)
        {
            tilemap.SetIsMoving(tilemap.tilemapType == TilemapType.GROUND);
        }
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;

        // tempo
        elapsedTime += Time.deltaTime;

        // --- calcula a velocidade atual do mundo (currentSpeed) ---
        // base
        currentSpeed = PitManager.Instance.IsPlayerInHell()? baseSpeed * hellSpeedMultiplier : baseSpeed;

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

        // esperamos um frame para garantir que o GameOverManager da cena já foi inicializado
        StartCoroutine(ShowGameOverNextFrame());
    }

    private IEnumerator ShowGameOverNextFrame()
    {
        // aguarda até o final do frame atual
        yield return null;

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver();
        }
        else
        {
            Debug.LogWarning("[GameManager] GameOverManager não encontrado na cena ao tentar mostrar GameOver.");
        }
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
