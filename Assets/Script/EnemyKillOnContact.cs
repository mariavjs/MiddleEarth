using UnityEngine;

public class EnemyKillOnContact : MonoBehaviour
{
    public AudioSource killSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Toca o som do inimigo (se tiver)
            if (killSound != null)
            {
                killSound.Play();
            }

            // Tenta pegar o script Player no objeto
            Player player = other.GetComponent<Player>();

            if (player != null)
            {
                // Chama o método de morte do player
                player.Die();
            }
            else
            {
                // Se por algum motivo não achar o script, destrói direto
                Destroy(other.gameObject);
            }
        }
    }
}
