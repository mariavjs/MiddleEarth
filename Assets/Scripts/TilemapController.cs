using UnityEngine;

public class TilemapController : MonoBehaviour
{
    private bool hasTriggered = false; // Cada tile só spawna uma vez

    void Update()
    {
        transform.Translate(-Vector2.right * 5f * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("[TilemapController] OnTriggerEnter2D: colidiu com " + other.name + " tag=" + other.tag);
        
        // Verifica se já spawnou e se tem a tag correta
        if (hasTriggered)
        {
            Debug.Log("[TilemapController] Este tile já spawnou um próximo, ignorando.");
            return;
        }
        
        // atravessou o gatilho (GameObject "TriggerSpawn" com Tag = "Box")
        if (other.CompareTag("Box"))
        {
            hasTriggered = true; // Marca que este tile já spawnou
            
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
    }
}
