using UnityEngine;

public class Coin : MonoBehaviour
{
    public AudioClip collectSound; // som curto de coleta
    private bool collected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Coin] OnTriggerEnter2D com: {other.name} (tag: {other.tag})");
        
        if (collected) return; // evita coletar duas vezes

        if (other != null && other.CompareTag("Player"))
        {
            collected = true;
            Debug.Log("[Coin] Player detectado! Coletando moeda...");

            // toca som (independe de AudioSource no objeto)
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, Camera.main != null ? Camera.main.transform.position : transform.position);
            }

            // adiciona +1 moeda no contador global (só se existir)
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.AddCoin();
                Debug.Log("[Coin] Moeda adicionada ao CoinManager.");
            }
            else
            {
                Debug.LogWarning("[Coin] CoinManager.Instance é null ao coletar moeda. Verifique se o CoinManager está na cena e ativo.");
            }

            // destrói a moeda imediatamente
            Destroy(gameObject);
        }
    }
}
