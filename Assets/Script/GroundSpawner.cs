using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    public GameObject groundPrefab;
    
    // Referência ao objeto de cena que marca onde spawnar o próximo tile.
    // Arraste o GameObject "NextSpawn" aqui no Inspector.
    public Transform nextSpawnMarker;
    
    private Vector3 nextTileSpawnPos;
    private bool isSpawning = false; // Previne múltiplos spawns simultâneos

    void Start()
    {
        // O primeiro tile já está na cena, então apenas inicializa a posição do próximo spawn
        if (nextSpawnMarker != null)
        {
            nextTileSpawnPos = nextSpawnMarker.position;
            Debug.Log("[GroundSpawner] Inicializado. Próximo tile será spawnado em: " + nextTileSpawnPos);
        }
        else
        {
            Debug.LogError("[GroundSpawner] nextSpawnMarker não atribuído! Arraste o objeto NextSpawn no Inspector.");
        }
    }

    public void SpawnTile()
    {
        if (isSpawning)
        {
            Debug.LogWarning("[GroundSpawner] Spawn já em andamento, ignorando chamada duplicada.");
            return;
        }

        isSpawning = true;

        // Instancia o próximo tile na posição do NextSpawn
        GameObject tile = Instantiate(groundPrefab, nextTileSpawnPos, Quaternion.identity);
        
        // Garante que o tile não tenha rotação (previne giros)
        tile.transform.rotation = Quaternion.identity;
        
        Debug.Log("[GroundSpawner] Tile spawnado em: " + nextTileSpawnPos);

        // Atualiza a posição do próximo spawn (continua usando o mesmo NextSpawn de cena)
        if (nextSpawnMarker != null)
        {
            nextTileSpawnPos = nextSpawnMarker.position;
            Debug.Log("[GroundSpawner] Próxima posição de spawn: " + nextTileSpawnPos);
        }
        else
        {
            Debug.LogWarning("[GroundSpawner] nextSpawnMarker não atribuído. Mantendo posição atual.");
        }

        isSpawning = false;
    }
}


// Start é chamado uma vez antes da primeira execução do Update após o MonoBehaviour ser criado
    // void Start()
    // {   
    //     nextTileSpawnPos = Vector3.zero; // Primeira posição é zero
    //     SpawnTile();
    // }

    // // Método para spawnar um novo tile
    // public void SpawnTile()
    // {
    //     // Instancia o groundPrefab na posição nextTileSpawnPos
    //     GameObject temp = Instantiate(groundPrefab, nextTileSpawnPos, Quaternion.identity);
    //     Debug.Log("Tile spawnado em: " + nextTileSpawnPos);

    //     // Busca o filho "NextSpawn" no tile instanciado
    //     Transform nextSpawn = temp.transform.Find("NextSpawn");
    //     if (nextSpawn != null)
    //     {
    //         // Atualiza nextTileSpawnPos com a posição de "NextSpawn"
    //         nextTileSpawnPos = nextSpawn.position;
    //         Debug.Log("Próxima posição de spawn atualizada para: " + nextTileSpawnPos);
    //     }
    //     else
    //     {
    //         Debug.LogError("Ground prefab não possui filho 'NextSpawn'.");
    //     }