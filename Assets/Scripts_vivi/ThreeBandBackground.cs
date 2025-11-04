using System.Collections.Generic;
using UnityEngine;

//[ExecuteAlways]
public class ThreeBandBackground : MonoBehaviour
{
    [System.Serializable]
    public class BandConfig
    {
        public string name = "Band";
        public float speed = 3f;
        public Color darkColor = Color.black;
        public Color lightColor = Color.white;

        [Tooltip("Sprites usadas nessa faixa (deixe em branco se quiser só cor)")]
        public Sprite darkSprite;
        public Sprite lightSprite;

        [HideInInspector] public float yCenter = 0f;
        [HideInInspector] public float bandHeight = 0f;
    }

    [Header("Prefab (1x1 sprite)")]
    public GameObject blockPrefab;

    [Header("Altura fixa de cada banda (em units) — cada faixa terá esta altura")]
    public float fixedBandHeight = 4f;

    [Header("Largura de cada bloco (world units). Deve ser maior que a largura da câmera")]
    public float blockWidth = 50f;

    [Header("Configura cada faixa — de cima pra baixo: Céu / Terra / Inferno")]
    public BandConfig[] bands = new BandConfig[3];

    [Header("Player para sincronizar a velocidade")]
    public Player player; // arraste o Player no Inspector

    private List<GameObject[]> bandBlocks = new List<GameObject[]>();
    private float totalHeight;

private float GetCameraWidth()
{
    var cam = Camera.main;
    if (cam == null) return 10f;
    return cam.orthographicSize * 2f * cam.aspect;
}
    void Awake()
{
    if (blockWidth <= 0f)
        blockWidth = GetCameraWidth() * 1.1f; // um pouco maior que a tela

    Setup();
}

    void OnValidate()
    {
        if (bands == null || bands.Length != 3)
        {
            var tmp = new BandConfig[3];
            for (int i = 0; i < 3; i++)
                tmp[i] = (i < bands?.Length ? bands[i] : new BandConfig());
            bands = tmp;
        }
    }
    

    public void Setup()
    {
        if (blockPrefab == null)
        {
            Debug.LogError("[ThreeBandBackground] BlockPrefab não atribuído.");
            return;
        }

        float bandH = Mathf.Max(0.01f, fixedBandHeight);
        totalHeight = bandH * 3f;

        // Limpa faixas antigas
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var t = transform.GetChild(i);
            if (Application.isPlaying) Destroy(t.gameObject);
            else DestroyImmediate(t.gameObject);
        }
        bandBlocks.Clear();

        // Calcula os centros (céu, terra, inferno)
        float topCenterY = bandH;
        float midCenterY = 0f;
        float botCenterY = -bandH;

        for (int i = 0; i < 3; i++)
        {
            var cfg = bands[i];
            cfg.bandHeight = bandH;
            cfg.yCenter = (i == 0) ? topCenterY : (i == 1 ? midCenterY : botCenterY);

            GameObject[] arr = new GameObject[2];

            for (int j = 0; j < 2; j++)
            {
                GameObject b = Instantiate(blockPrefab, transform);
                b.name = $"Band_{i}_Block_{j}_{cfg.name}";
                b.transform.position = new Vector3(j * blockWidth, cfg.yCenter, 0f);

                var sr = b.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // Escolhe qual sprite usar
                    Sprite chosenSprite = (j % 2 == 0) ? cfg.darkSprite : cfg.lightSprite;
                    if (chosenSprite != null)
                    {
                        sr.sprite = chosenSprite;
                        sr.drawMode = SpriteDrawMode.Tiled; // Faz o tile automático
                        sr.size = new Vector2(blockWidth, bandH);
                        sr.color = Color.white; // Garante que não tinge a sprite
                    }
                    else
                    {
                        sr.sprite = null;
                        sr.drawMode = SpriteDrawMode.Simple;
                        sr.color = (j % 2 == 0) ? cfg.darkColor : cfg.lightColor;
                    }
                }

                arr[j] = b;
            }

            bandBlocks.Add(arr);
        }
    }

    void Update()
    {
        if (blockPrefab == null || bands == null || bands.Length != 3) return;

        // // 🔹 Controla a velocidade das faixas com base no Player
        // if (player != null)
        // {
        //     bands[0].speed = player.speed * 0.3f;   // Céu = mais lento
        //     bands[1].speed = player.speed;          // Terra = igual ao player
        //     bands[2].speed = player.speed * 1.2f;   // Inferno = mais rápido
        // }

        for (int i = 0; i < bands.Length; i++)
        {
            var cfg = bands[i];
            var blocks = bandBlocks.Count > i ? bandBlocks[i] : null;
            if (blocks == null) continue;

            float move = cfg.speed * Time.deltaTime;

            // Move cada bloco
            for (int k = 0; k < blocks.Length; k++)
                blocks[k].transform.position += Vector3.left * move;

            // Reposiciona quando sai da tela
            for (int k = 0; k < blocks.Length; k++)
            {
                var b = blocks[k];
                if (b.transform.position.x <= -blockWidth)
                {
                    float rightMost = float.MinValue;
                    foreach (var bb in blocks)
                        if (bb.transform.position.x > rightMost)
                            rightMost = bb.transform.position.x;

                    b.transform.position = new Vector3(rightMost + blockWidth, cfg.yCenter, 0f);
                }
            }
        }
    }

    [ContextMenu("Rebuild Bands")]
    public void Rebuild() => Setup();

    public float GetBandCenterY(int index)
    {
        if (bands == null || index < 0 || index >= bands.Length) return 0f;
        return bands[index].yCenter;
    }

    public float GetBandHeight(int index)
    {
        if (bands == null || index < 0 || index >= bands.Length) return fixedBandHeight;
        return bands[index].bandHeight;
    }
}
