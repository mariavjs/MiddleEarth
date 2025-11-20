using System.Collections.Generic;
using UnityEngine;

public class ThreeBandBackground : MonoBehaviour
{

    // --- coloque dentro da classe TwoBandBackground ou ThreeBandBackground ---
public Player player; // referência pro jogador

// retorna altura da banda (pode usar fixedBandHeight)
public float GetBandHeight(int index)
{
    return fixedBandHeight;
}

// retorna a posição vertical da banda
public float GetBandCenterY(int index)
{
    if (bands == null || index < 0 || index >= bands.Length) return 0f;
    return bands[index].yCenter;
}
    [System.Serializable]
    public class BandConfig
    {
        public string name = "Band";
        public float speed = 3f;
        [Tooltip("Sprite da esquerda")]
        public Sprite leftSprite;
        [Tooltip("Sprite da direita (opcional, se quiser alternar)")]
        public Sprite rightSprite;
        [HideInInspector] public float yCenter = 0f;
    }

    [Header("Prefab base (deve conter SpriteRenderer)")]
    public GameObject blockPrefab;

    [Header("Altura fixa de cada banda (em units)")]
    public float fixedBandHeight = 4f;

    [Header("Largura de cada bloco (world units)")]
    public float blockWidth = 25f;

    [Header("Bandas — de cima pra baixo: Terra / Inferno")]
    public BandConfig[] bands = new BandConfig[2];

    private List<GameObject[]> bandBlocks = new List<GameObject[]>();

    private float GetCameraWidth()
    {
        var cam = Camera.main;
        if (cam == null) return 10f;
        return cam.orthographicSize * 2f * cam.aspect;
    }

    void Awake()
    {
        if (blockWidth <= 0f)
            blockWidth = GetCameraWidth() * 1.1f;

        Setup();
    }

    [ContextMenu("Rebuild Bands")]
    public void Rebuild() => Setup();

    public void Setup()
    {
        if (blockPrefab == null)
        {
            Debug.LogError("[TwoBandBackground] BlockPrefab não atribuído.");
            return;
        }

        float bandH = Mathf.Max(0.01f, fixedBandHeight);

        // Limpa faixas antigas
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var t = transform.GetChild(i);
            if (Application.isPlaying) Destroy(t.gameObject);
            else DestroyImmediate(t.gameObject);
        }
        bandBlocks.Clear();

        // Calcula posições Y: terra no topo, inferno abaixo
        float topY = 0f;
        float bottomY = -bandH;

        for (int i = 0; i < bands.Length; i++)
        {
            var cfg = bands[i];
            cfg.yCenter = (i == 0) ? topY : bottomY;

            GameObject[] arr = new GameObject[2];

            for (int j = 0; j < 2; j++)
            {
                GameObject b = Instantiate(blockPrefab, transform);
                b.name = $"Band_{i}_Block_{j}_{cfg.name}";
                b.transform.position = new Vector3(j * blockWidth, cfg.yCenter, 0f);

                var sr = b.GetComponent<SpriteRenderer>();
                if (sr == null)
                {
                    Debug.LogError("Prefab precisa ter SpriteRenderer!");
                    continue;
                }

                Sprite spriteToUse = (j % 2 == 0) ? cfg.leftSprite : cfg.rightSprite ?? cfg.leftSprite;
                sr.sprite = spriteToUse;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(blockWidth, bandH);
                sr.color = Color.white;
            }

            bandBlocks.Add(arr);
        }
    }

    void Update()
    {
        if (bandBlocks.Count == 0) return;

        for (int i = 0; i < bands.Length; i++)
        {
            var cfg = bands[i];
            var blocks = bandBlocks[i];
            float move = cfg.speed * Time.deltaTime;

            // Move blocos para a esquerda
            for (int k = 0; k < blocks.Length; k++)
                blocks[k].transform.position += Vector3.left * move;

            // Reposiciona blocos que saíram da tela
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
}
