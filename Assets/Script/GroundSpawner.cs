using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    [System.Serializable]
    public class TileLayer
    {
        public string layerName = "Ground Layer";
        public GameObject[] tilePrefabs;
        public Transform nextSpawnMarker;
        [HideInInspector] public Vector3 nextTileSpawnPos;
        [HideInInspector] public bool isSpawning = false;
    }

    [Header("Configuração de Layers")]
    public TileLayer[] tileLayers; // Pode ter Ground, Hell, etc.

    void Start()
    {
        // Inicializa cada layer
        foreach (TileLayer layer in tileLayers)
        {
            if (layer.tilePrefabs == null || layer.tilePrefabs.Length == 0)
            {
                Debug.LogError($"[GroundSpawner] Layer '{layer.layerName}': Nenhum prefab atribuído!");
                continue;
            }

            if (layer.nextSpawnMarker != null)
            {
                layer.nextTileSpawnPos = layer.nextSpawnMarker.position;
                // Debug.Log($"[GroundSpawner] Layer '{layer.layerName}' inicializado. Próximo spawn em: {layer.nextTileSpawnPos}");
            }
            else
            {
                Debug.LogError($"[GroundSpawner] Layer '{layer.layerName}': nextSpawnMarker não atribuído!");
            }
        }
    }

    // Spawna tile em um layer específico (0 = primeiro layer, 1 = segundo, etc.)
    public void SpawnTile(int layerIndex = 0)
    {
        Debug.Log($"[GroundSpawner] SpawnTile chamado com layerIndex: {layerIndex}");
        
        if (layerIndex < 0 || layerIndex >= tileLayers.Length)
        {
            Debug.LogError($"[GroundSpawner] Layer index {layerIndex} inválido! Total de layers: {tileLayers.Length}");
            return;
        }

        TileLayer layer = tileLayers[layerIndex];
        Debug.Log($"[GroundSpawner] Spawnando no layer '{layer.layerName}' (index {layerIndex})");

        if (layer.isSpawning)
        {
            Debug.LogWarning($"[GroundSpawner] Layer '{layer.layerName}': Spawn já em andamento, ignorando.");
            return;
        }

        if (layer.tilePrefabs == null || layer.tilePrefabs.Length == 0)
        {
            Debug.LogError($"[GroundSpawner] Layer '{layer.layerName}': Nenhum prefab disponível!");
            return;
        }

        layer.isSpawning = true;

        // Escolhe um prefab aleatório
        int randomIndex = Random.Range(0, layer.tilePrefabs.Length);
        GameObject selectedPrefab = layer.tilePrefabs[randomIndex];

        // Instancia
        GameObject tile = Instantiate(selectedPrefab, layer.nextTileSpawnPos, Quaternion.identity);
        tile.transform.rotation = Quaternion.identity;
        
        // Debug.Log($"[GroundSpawner] Layer '{layer.layerName}': Tile '{selectedPrefab.name}' spawnado em {layer.nextTileSpawnPos}");

        // Atualiza próxima posição
        if (layer.nextSpawnMarker != null)
        {
            layer.nextTileSpawnPos = layer.nextSpawnMarker.position;
        }

        layer.isSpawning = false;
    }

    // Versão antiga para compatibilidade (spawna no layer 0)
    public void SpawnTile()
    {
        SpawnTile(0);
    }

    public void DestroyTile(GameObject tile)
    {
        if (tile != null)
        {
            // Debug.Log($"[GroundSpawner] Destruindo tile: {tile.name}");
            Destroy(tile);
        }
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