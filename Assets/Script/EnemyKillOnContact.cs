using UnityEngine;

public class EnemyKillOnContact : MonoBehaviour
{
    [Header("Configuração do inimigo")]
    public int damage = 1; // Eye = 1, Touro = 2

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Enemy] OnTriggerEnter2D com: {other.name} (tag: {other.tag})");
        
        // Garante que só reage ao Player
        if (!other.CompareTag("Player")) return;

        Debug.Log("[Enemy] Player detectado! Aplicando dano...");

        // Acessa o script Player e aplica dano
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Debug.Log($"[Enemy] Dano {damage} aplicado ao Player.");
        }
        else
        {
            Debug.LogWarning("[Enemy] Player não tem componente Player!");
        }
    }
}
