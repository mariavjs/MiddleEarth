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
        if (layerIndex < 0 || layerIndex >= tileLayers.Length)
        {
            Debug.LogError($"[GroundSpawner] Layer index {layerIndex} inválido! Total de layers: {tileLayers.Length}");
            return;
        }

        TileLayer layer = tileLayers[layerIndex];

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
        TilemapController tile = Instantiate(selectedPrefab, layer.nextTileSpawnPos, Quaternion.identity).GetComponent<TilemapController>();
        tile.transform.rotation = Quaternion.identity;

        tile.tilemapType = layerIndex == 0? TilemapType.GROUND : TilemapType.HELL;

        if (PitManager.Instance.IsPlayerInHell())
        {
            tile.SetIsMoving(tile.tilemapType == TilemapType.HELL);
        }
        else
        {
            tile.SetIsMoving(tile.tilemapType == TilemapType.GROUND);
        }

        // Atualiza próxima posição
        if (layer.nextSpawnMarker != null)
        {
            layer.nextTileSpawnPos = layer.nextSpawnMarker.position;
        }

        layer.isSpawning = false;
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