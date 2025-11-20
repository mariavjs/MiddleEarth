// Assets/Scripts/ShopManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("UI refs")]
    public TextMeshProUGUI playerCoinsText;
    public TextMeshProUGUI infernoDurationText;
    public TextMeshProUGUI lifePriceText;
    public TextMeshProUGUI infernoPriceText;
    public Button buyLifeButton;
    public Button buyInfernoButton;
    public Button backToMenuButton;

    [Header("Prices & settings")]
    public int priceLife = 50;
    public int priceInferno = 50;
    public float infernoReduceSeconds = 5f;

    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";
    public int mainMenuIndexFallback = 0;

    void Start()
    {
        if (buyLifeButton != null) buyLifeButton.onClick.AddListener(OnBuyLife);
        if (buyInfernoButton != null) buyInfernoButton.onClick.AddListener(OnBuyInferno);
        if (backToMenuButton != null) backToMenuButton.onClick.AddListener(OnBackToMainMenu);

        if (lifePriceText != null) lifePriceText.text = priceLife + "c";
        if (infernoPriceText != null) infernoPriceText.text = priceInferno + "c";

        // garante que InfernoConfig carregou
        InfernoConfig.Load();

        UpdateUI();
    }

    void Update()
    {
        // atualiza moedas e botões em tempo real
        UpdateUI();
    }

    void UpdateUI()
    {
        int coins = GetTotalCoins();
        if (playerCoinsText != null) playerCoinsText.text = "Coins: " + coins;
        if (infernoDurationText != null) infernoDurationText.text = "Inferno: " + Mathf.FloorToInt(InfernoConfig.InfernoDurationSeconds) + "s";

        if (buyLifeButton != null) buyLifeButton.interactable = (coins >= priceLife);
        if (buyInfernoButton != null) buyInfernoButton.interactable = (coins >= priceInferno);
    }

    public void OnBuyLife()
    {
        if (!TrySpendCoins(priceLife))
        {
            Debug.Log("[ShopManager] Saldo insuficiente para comprar vida.");
            return;
        }

        // aplica vida via GameManager.player
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            int added = GameManager.Instance.player.AddLife(1);
            Debug.Log("[ShopManager] Vida comprada. Vidas adicionadas: " + added);
        }
        else
        {
            Debug.LogWarning("[ShopManager] GameManager.player não encontrado. Vida comprada mas não aplicada ao player.");
        }

        UpdateUI();
    }

    public void OnBuyInferno()
    {
        if (!TrySpendCoins(priceInferno))
        {
            Debug.Log("[ShopManager] Saldo insuficiente para comprar redução do inferno.");
            return;
        }

        InfernoConfig.ReduceInfernoDuration(infernoReduceSeconds);
        Debug.Log("[ShopManager] Inferno reduzido. Novo valor: " + InfernoConfig.InfernoDurationSeconds);
        UpdateUI();
    }

    public void OnBackToMainMenu()
    {
        // troca direto para o menu principal
        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            SceneManager.LoadScene(mainMenuIndexFallback);
    }

    // ------------ moedas (usa CoinManager se disponível) ------------
    int GetTotalCoins()
    {
        if (CoinManager.Instance != null) return CoinManager.Instance.GetTotalCoins();
        // fallback PlayerPrefs
        return PlayerPrefs.GetInt("PLAYER_COINS", PlayerPrefs.GetInt("TotalCoins", 0));
    }

    bool TrySpendCoins(int amount)
    {
        if (amount <= 0) return true;
        if (CoinManager.Instance != null)
        {
            bool ok = CoinManager.Instance.TrySpendCoins(amount);
            if (ok) Debug.Log("[ShopManager] Gastou " + amount + " via CoinManager.");
            return ok;
        }

        int current = GetTotalCoins();
        if (current >= amount)
        {
            int next = current - amount;
            PlayerPrefs.SetInt("PLAYER_COINS", next);
            PlayerPrefs.Save();
            Debug.Log("[ShopManager] Gastou " + amount + " via PlayerPrefs. Novo total: " + next);
            return true;
        }

        return false;
    }
}
