using UnityEngine;

public class TilemapController : MonoBehaviour
{
    private bool hasTriggeredSpawn = false; // Cada tile só spawna uma vez
    private bool hasTriggeredDestroy = false; // Cada tile só se destrói uma vez

    void Update()
    {
        transform.Translate(-Vector2.right * 5f * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("[TilemapController] OnTriggerEnter2D: colidiu com " + other.name + " tag=" + other.tag);
        
        // Detecta colisão com TriggerSpawn (tag = "Box") para spawnar próximo tile
        if (other.CompareTag("Box") && !hasTriggeredSpawn)
        {
            hasTriggeredSpawn = true;
            
            var spawner = FindObjectOfType<GroundSpawner>();
            if (spawner)
            {
                spawner.SpawnTile();
            }
            else
            {
                Debug.LogError("[TilemapController] GroundSpawner não encontrado na cena.");
            }
        }
        
        // Detecta colisão com DestroySpawn (tag = "Destroy") para destruir este tile
        if (other.CompareTag("Destroy") && !hasTriggeredDestroy)
        {
            hasTriggeredDestroy = true;
            
            var spawner = FindObjectOfType<GroundSpawner>();
            if (spawner)
            {
                spawner.DestroyTile(this.gameObject);
            }
            else
            {
                Debug.LogError("[TilemapController] GroundSpawner não encontrado para destruir tile.");
            }
        }
    }
}
