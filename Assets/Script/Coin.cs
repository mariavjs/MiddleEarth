using UnityEngine;

public class Coin : MonoBehaviour
{
    public AudioClip collectSound; // som curto de coleta
    private bool collected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return; // evita coletar duas vezes

        if (other.CompareTag("Player"))
        {
            collected = true;

            // toca som (não depende de AudioSource no objeto)
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }

            // adiciona +1 moeda no contador global
            CoinManager.Instance.AddCoin();

            // desativa (ou destrói) a moeda
            Destroy(gameObject);
        }
    }
}
