using System.Collections.Generic;
using UnityEngine;

// [ExecuteAlways]
public class ThreeBandBackground : MonoBehaviour
{
    [System.Serializable]
    public class BandConfig
    {
        public string name = "Band";
        public float speed = 3f;
        public Color darkColor = Color.black;
        public Color lightColor = Color.white;
        [HideInInspector] public float yCenter = 0f;
        [HideInInspector] public float bandHeight = 0f;
    }

    [Header("Prefab (1x1 sprite)")]
    public GameObject BlockPrefab;

    [Header("Configura cada faixa — de cima para baixo: Céu / Terra / Inferno")]
    public BandConfig[] bands = new BandConfig[3];

    private List<GameObject[]> bandBlocks = new List<GameObject[]>();
    private float camWidth;
    private float camHeight;

    void Start()
    {
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

    void Setup()
    {
        if (BlockPrefab == null)
        {
            Debug.LogError("[ThreeBandBackground] BlockPrefab não atribuído.");
            return;
        }

        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic)
        {
            Debug.LogError("[ThreeBandBackground] Main Camera não encontrada ou não ortográfica.");
            return;
        }

        camHeight = cam.orthographicSize * 2f;
        camWidth = camHeight * cam.aspect;
        float bandH = camHeight / 3f;
        float camY = cam.transform.position.y;
        float half = camHeight / 2f;
        float topCenterY = camY + half - bandH / 2f;
        float midCenterY = camY;
        float botCenterY = camY - half + bandH / 2f;

        foreach (Transform t in transform)
        {
            if (Application.isPlaying) Destroy(t.gameObject);
            else DestroyImmediate(t.gameObject);
        }
        bandBlocks.Clear();

        for (int i = 0; i < 3; i++)
        {
            var cfg = bands[i];
            cfg.bandHeight = bandH;
            cfg.yCenter = i == 0 ? topCenterY : (i == 1 ? midCenterY : botCenterY);

            GameObject[] arr = new GameObject[2];
            for (int j = 0; j < 2; j++)
            {
                GameObject b = Instantiate(BlockPrefab, transform);
                b.name = $"Band_{i}_Block_{j}_{cfg.name}";
                b.transform.position = new Vector3(j * camWidth, cfg.yCenter, 0f);
                b.transform.localScale = new Vector3(camWidth, bandH, 1f);

                var sr = b.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = (j % 2 == 0) ? cfg.darkColor : cfg.lightColor;
                arr[j] = b;
            }
            bandBlocks.Add(arr);
        }
    }

    private void Update()
    {
        if (BlockPrefab == null) return;
        if (bands == null || bands.Length != 3) return;

        Camera cam = Camera.main;
        if (cam == null) return;
        float newCamHeight = cam.orthographicSize * 2f;
        float newCamWidth = newCamHeight * cam.aspect;
        if (Mathf.Abs(newCamHeight - camHeight) > 0.01f || Mathf.Abs(newCamWidth - camWidth) > 0.01f || bandBlocks.Count != 3)
        {
            camHeight = newCamHeight;
            camWidth = newCamWidth;
            Setup();
        }

        for (int i = 0; i < bands.Length; i++)
        {
            var cfg = bands[i];
            var blocks = bandBlocks[i];
            float move = cfg.speed * Time.deltaTime;

            for (int k = 0; k < blocks.Length; k++)
                blocks[k].transform.position += Vector3.left * move;

            for (int k = 0; k < blocks.Length; k++)
            {
                var b = blocks[k];
                if (b.transform.position.x <= -camWidth)
                {
                    float rightMost = float.MinValue;
                    foreach (var bb in blocks) if (bb.transform.position.x > rightMost) rightMost = bb.transform.position.x;
                    b.transform.position = new Vector3(rightMost + camWidth, cfg.yCenter, 0f);

                    var sr = b.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        if (ApproximatelyColor(sr.color, cfg.darkColor)) sr.color = cfg.lightColor;
                        else sr.color = cfg.darkColor;
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
}
