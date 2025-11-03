using UnityEngine;

public class CameraPlaneSwitcher : MonoBehaviour
{
    public ThreeBandBackground background; // arraste aqui o BackgroundController
    public float moveSpeed = 3f; // quanto mais alto, mais rápido o lerp
    public float zoomSmooth = 6f; // se quiser animar orthographic size

    private int currentBand = 1; // começa na Terra
    private float targetY;
    private Camera cam;

    void Start()
    {
        if (background == null)
        {
            Debug.LogError("CameraPlaneSwitcher: atribua o BackgroundController no Inspector!");
            enabled = false;
            return;
        }

        cam = Camera.main;
        if (cam == null || !cam.orthographic)
        {
            Debug.LogError("CameraPlaneSwitcher: Main Camera não encontrada ou não é ortográfica.");
            enabled = false;
            return;
        }

        // configurar tamanho da camera (apenas uma vez)
        float bandH = background.GetBandHeight(1);
        if (bandH > 0f)
            cam.orthographicSize = bandH / 2f;

        // posiciona no centro da banda do meio
        currentBand = 1;
        targetY = background.GetBandCenterY(currentBand);
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
    }

    void Update()
    {
        bool changed = false;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentBand == 1) { currentBand = 0; changed = true; }
            else if (currentBand == 2) { currentBand = 1; changed = true; }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentBand == 1) { currentBand = 2; changed = true; }
            else if (currentBand == 0) { currentBand = 1; changed = true; }
        }

        if (changed)
        {
            targetY = background.GetBandCenterY(currentBand);
            // opcional: se quiser animar orthographicSize ao mudar de banda com zoom:
            // float targetSize = background.GetBandHeight(currentBand) / 2f;
            // StartCoroutine(AnimateCameraSize(cam.orthographicSize, targetSize));
        }

        // movimento suave vertical (Lerp)
        float newY = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * moveSpeed);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    // opcional: coroutine para animar size (se quiser)
    // IEnumerator AnimateCameraSize(float from, float to) { ... }
}
