// CoinManager.cs
using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    private int sessionCoins = 0;     // moedas coletadas nesta partida
    private int totalCoins = 0;       // saldo persistido do jogador

    public TextMeshProUGUI coinText; 
    private const string COINS_KEY = "PLAYER_COINS"; // PlayerPrefs key

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // opcional se quiser persistir CoinManager entre cenas
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        LoadTotalCoins();
        // reset session coins at the start of a round
        ResetSessionCoins(); // ResetSessionCoins will call UpdateUI()
    }

    // chamada pelas moedas (Coin.cs)
    public void AddCoin(int amount = 1)
    {
        sessionCoins += amount;
        totalCoins += amount;

        UpdateUI();
        SaveTotalCoins();
    }

    // reseta a contagem da sessão (por exemplo, no início de cada partida)
    public void ResetSessionCoins()
    {
        sessionCoins = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (coinText != null)
        {
            // mostra o ícone (pode ser emoji) seguido do saldo total ou da sessão
            // escolha: mostrar total persistido para shop: totalCoins
            // ou mostrar sessão: sessionCoins
            // aqui eu mostro total acumulado (para shop) e sessão entre parenteses:
            // show session coins on the HUD; keep totalCoins persisted for shop use
            coinText.text = sessionCoins.ToString();
        }
    }

    private void SaveTotalCoins()
    {
        PlayerPrefs.SetInt(COINS_KEY, totalCoins);
        PlayerPrefs.Save();
    }

    private void LoadTotalCoins()
    {
        totalCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
    }

    // utilitários públicos
    public int GetTotalCoins() => totalCoins;
    public int GetSessionCoins() => sessionCoins;

    // opcional: gastar moedas (usado pelo Shop)
    public bool TrySpendCoins(int cost)
    {
        if (totalCoins >= cost)
        {
            totalCoins -= cost;
            SaveTotalCoins();
            UpdateUI();
            return true;
        }
        return false;
    }
}
