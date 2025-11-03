using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ThreeBandBackground : MonoBehaviour
{
    [System.Serializable]
    public class BandConfig
    {
        public string name = "Band";
        public float speed = 3f;
        public Color darkColor = Color.black;
        public Color lightColor = Color.white;

        [Tooltip("Se quiser sprite em vez de cor (opcional)")]
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

    private List<GameObject[]> bandBlocks = new List<GameObject[]>();
    private float totalHeight;

    void Awake()
    {
        // Garante que as bandas existam ao iniciar (editor ou play)
        Setup();
    }

    void OnValidate()
    {
        if (bands == null || bands.Length != 3)
        {
            var tmp = new BandConfig[3];
            for (int i = 0; i < 3; i++) tmp[i] = (i < bands?.Length ? bands[i] : new BandConfig());
            bands = tmp;
        }
    }

    public void Setup()
    {
        if (blockPrefab == null)
        {
            Debug.LogError("[ThreeBandBackground] blockPrefab não atribuído.");
            return;
        }

        // definimos a bandHeight fixa (mesma para todas as bandas)
        float bandH = Mathf.Max(0.01f, fixedBandHeight);
        totalHeight = bandH * 3f;

        // limpar filhos antigos
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var t = transform.GetChild(i);
            if (Application.isPlaying) Destroy(t.gameObject);
            else DestroyImmediate(t.gameObject);
        }
        bandBlocks.Clear();

        // calcular centros Y (de cima pra baixo)
        float topCenterY = bandH;                 // banda superior centrada em +bandH (ajuste arbitrário)
        float midCenterY = 0f;                    // centraliza a banda do meio em 0
        float botCenterY = -bandH;

        // aqui escolhi centragem: top = +bandH, mid = 0, bot = -bandH
        // ajuste se quiser outra organização (ex: top = bandH, mid = -bandH, etc.)

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

                // posicionar horizontalmente; blocos são longos para cobrir (poderia ser mais do que 2 se quiser)
                b.transform.position = new Vector3(j * blockWidth, cfg.yCenter, 0f);
                b.transform.localScale = new Vector3(blockWidth, bandH, 1f);

                var sr = b.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // se tiver sprite configurado, usa; senão usa cor
                    if (j % 2 == 0)
                    {
                        if (cfg.darkSprite != null) sr.sprite = cfg.darkSprite;
                        sr.color = cfg.darkSprite == null ? cfg.darkColor : Color.white;
                    }
                    else
                    {
                        if (cfg.lightSprite != null) sr.sprite = cfg.lightSprite;
                        sr.color = cfg.lightSprite == null ? cfg.lightColor : Color.white;
                    }
                }

                arr[j] = b;
            }
            bandBlocks.Add(arr);
        }
    }

    void Update()
    {
        // movimento horizontal das faixas (endless runner)
        if (blockPrefab == null) return;
        if (bands == null || bands.Length != 3) return;

        for (int i = 0; i < bands.Length; i++)
        {
            var cfg = bands[i];
            var blocks = bandBlocks.Count > i ? bandBlocks[i] : null;
            if (blocks == null) continue;
            float move = cfg.speed * Time.deltaTime;

            for (int k = 0; k < blocks.Length; k++)
                blocks[k].transform.position += Vector3.left * move;

            for (int k = 0; k < blocks.Length; k++)
            {
                var b = blocks[k];
                // se passou muito para a esquerda, reposiciona à direita
                if (b.transform.position.x <= -blockWidth)
                {
                    float rightMost = float.MinValue;
                    foreach (var bb in blocks) if (bb.transform.position.x > rightMost) rightMost = bb.transform.position.x;
                    b.transform.position = new Vector3(rightMost + blockWidth, cfg.yCenter, 0f);

                    // alterna cor / sprite se quiser
                    var sr = b.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        // só alterna cor caso esteja usando cor (se sprite != null, mantem)
                        if (sr.sprite == null)
                        {
                            if (ApproximatelyColor(sr.color, cfg.darkColor)) sr.color = cfg.lightColor;
                            else sr.color = cfg.darkColor;
                        }
                    }
                }
            }
        }
    }

    private bool ApproximatelyColor(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.01f && Mathf.Abs(a.g - b.g) < 0.01f && Mathf.Abs(a.b - b.b) < 0.01f && Mathf.Abs(a.a - b.a) < 0.01f;
    }

    [ContextMenu("Rebuild Bands")]
    public void Rebuild()
    {
        Setup();
    }

    // expõe para outros scripts
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
