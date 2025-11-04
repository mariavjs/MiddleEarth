using UnityEngine;

public class EnemyKillSimple : MonoBehaviour
{
    [Tooltip("Se true, o player será apenas desativado (SetActive false). Se false, será destruído (Destroy).")]
    public bool deactivateInsteadOfDestroy = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // opção: desativar (bom para debug) ou destruir
            if (deactivateInsteadOfDestroy)
            {
                other.gameObject.SetActive(false);
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
    }
}
