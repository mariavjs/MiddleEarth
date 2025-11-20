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

    [Header("Ajustes")]
    public float distanceMultiplier = 1f;        // converte unidades internas para "m"
    public bool increaseSpeedWithDistance = false;
    public float speedIncreaseDistance = 100f;   // a cada X metros aplica aumento
    public float speedIncreaseAmount = 0.5f;     // valor adicionado à player.speed
    public bool useMultiplicativeIncrease = false;
    public float speedIncreaseMultiplier = 1.05f;

    // estado
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
    }

    void Update()
    {
        // não atualiza quando o jogo está "pausado" (timeScale 0)
        if (Time.timeScale <= 0f) return;

        // tempo
        elapsedTime += Time.deltaTime;

        // velocidade usada para calcular distância
        float currentSpeed = (player != null) ? player.speed : 0f;

        // distância
        distance += currentSpeed * Time.deltaTime * distanceMultiplier;

        // aumento de velocidade por distância (opcional)
        if (increaseSpeedWithDistance && player != null && distance >= nextSpeedIncreaseAt)
        {
            ApplySpeedIncrease();
            nextSpeedIncreaseAt += speedIncreaseDistance;
        }

        UpdateUI();
    }

    void ApplySpeedIncrease()
    {
        if (player == null) return;
        if (useMultiplicativeIncrease) player.speed *= speedIncreaseMultiplier;
        else player.speed += speedIncreaseAmount;
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
        // você pode abrir o painel de GameOver aqui
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

    public void ResetStats()
    {
        elapsedTime = 0f;
        distance = 0f;
        nextSpeedIncreaseAt = speedIncreaseDistance;
        UpdateUI();
    }

    // exemplo: restart via botão
    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
