using UnityEngine;

public class EnemyKillOnContact : MonoBehaviour
{
    [Header("Configuração do inimigo")]
    public int damage = 1; // Eye = 1, Touro = 2

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Garante que só reage ao Player
        if (!other.CompareTag("Player")) return;

        // Acessa o script Player e aplica dano
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }
    }
}
