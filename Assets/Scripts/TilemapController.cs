using UnityEngine;

public class TilemapController : MonoBehaviour
{
    [Header("Movimento")]
    [Tooltip("Velocidade local usada quando não há GameManager (unidades/s).")]
    public float baseSpeed = 5f;

    [Tooltip("Multiplica a velocidade do mundo para este objeto (ex: 0.5 = metade, 2 = dobro).")]
    public float speedMultiplier = 1f;

    [Tooltip("Se true, respeita o Time.timeScale (não se move quando tempo está parado).")]
    public bool respectTimeScale = true;

    [Header("Spawn Layer")]
    [Tooltip("Qual layer de tiles este prefab deve spawnar (0 = primeiro, 1 = segundo, etc.)")]
    public int spawnLayerIndex = 0;

    private bool hasTriggeredSpawn = false; // Cada tile só spawna uma vez
    private bool hasTriggeredDestroy = false; // Cada tile só se destrói uma vez

    void Start()
    {
        // Desabilita scripts MoveLeft nos filhos para evitar movimento duplicado
        // (os filhos já se movem junto com o pai Ground automaticamente)
        MoveLeft[] childMoveScripts = GetComponentsInChildren<MoveLeft>();
        foreach (MoveLeft moveScript in childMoveScripts)
        {
            if (moveScript.gameObject != this.gameObject) // Não desabilita se estiver no próprio objeto
            {
                moveScript.enabled = false;
                // Debug.Log($"[TilemapController] Desabilitado MoveLeft em filho: {moveScript.gameObject.name}");
            }
        }
    }

    void Update()
    {
        // pausa segura
        if (respectTimeScale && Time.timeScale <= 0f) return;

        // pega a velocidade do mundo (se disponível) ou usa a base local
        float worldSpeed = baseSpeed;
        if (GameManager.Instance != null)
        {
            worldSpeed = GameManager.Instance.GetCurrentSpeed();
            // caso GameManager retorne 0 (por algum motivo), mantém fallback:
            if (Mathf.Approximately(worldSpeed, 0f))
                worldSpeed = baseSpeed;
        }

        // aplica multiplicador local e move para a esquerda
        float finalSpeed = worldSpeed * speedMultiplier;
        
        // DEBUG: Log a cada 60 frames (~1 segundo)
        // if (Time.frameCount % 60 == 0)
        // {
        //     Debug.Log($"[TilemapController] {gameObject.name} - baseSpeed: {baseSpeed}, worldSpeed: {worldSpeed}, multiplier: {speedMultiplier}, finalSpeed: {finalSpeed}");
        // }
        
        transform.Translate(Vector2.left * finalSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log("[TilemapController] OnTriggerEnter2D: colidiu com " + other.name + " tag=" + other.tag);
        
        // Detecta colisão com TriggerSpawn (tag = "Box") para spawnar próximo tile
        if (other.CompareTag("Box") && !hasTriggeredSpawn)
        {
            hasTriggeredSpawn = true;
            
            Debug.Log($"[TilemapController] {gameObject.name} detectou TriggerSpawn! Spawnando layer index: {spawnLayerIndex}");
            
            var spawner = FindObjectOfType<GroundSpawner>();
            if (spawner)
            {
                // Spawna no layer configurado neste prefab
                spawner.SpawnTile(spawnLayerIndex);
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
