using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Referências")]
    public Player player;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI distanceText;

    [Header("Configurações")]
    public float distanceMultiplier = 1f;
    private float elapsedTime = 0f;
    private float distance = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;

        elapsedTime += Time.deltaTime;
        float speed = (player != null) ? player.speed : 5f;
        distance += speed * Time.deltaTime * distanceMultiplier;

        UpdateUI();
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
}
