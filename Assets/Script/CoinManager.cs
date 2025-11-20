using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance; // Singleton

    private int coinCount = 0;
    public TextMeshProUGUI coinText; // <-- aqui o tipo muda

void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject); // mantém o objeto entre cenas
    }
    else
    {
        Destroy(gameObject);
    }
}

    void Start()
    {
        UpdateUI();
    }

    public void AddCoin()
    {
        coinCount++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (coinText != null)
        {
            // imprime contagem das moedas
            coinText.text = coinCount.ToString();
        }
    }

    public void ResetCoins()
    {
        coinCount = 0;
        UpdateUI();
    }
}
