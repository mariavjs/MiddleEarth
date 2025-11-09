using UnityEngine;

public class EnemyKillOnContact : MonoBehaviour
{
    [Header("Configuração do inimigo")]
    public int damage = 1; // eye = 1, touro = 2
    public AudioSource killSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (killSound != null)
                killSound.Play();

            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }
}
