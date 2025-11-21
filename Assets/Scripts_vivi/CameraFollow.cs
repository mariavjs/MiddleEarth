using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;

    [Header("Ajustes de movimento")]
    public float followSmoothTime = 0.12f;
    public float transitionTime = 0.25f;
    public float groundYOffset = 0f;  // offset em relação ao player na terra
    public float hellCameraY = -12f;  // posição fixa da câmera no inferno
    public bool keepXFollowing = true;

    private Vector3 velocity = Vector3.zero;
    private bool inHell = false;
    private Coroutine transCoroutine;

    void Start()
    {
        if (player != null)
        {
            // inicializa a posição da câmera no começo do jogo
            transform.position = new Vector3(
                player.position.x,
                player.position.y + groundYOffset,
                transform.position.z
            );
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Eixo X (segue o player)
        float targetX = keepXFollowing ? player.position.x : transform.position.x;

        // Eixo Y
        //float targetY = inHell ? hellCameraY : (player.position.y + groundYOffset);
        float targetY = inHell ? hellCameraY : groundYOffset;

        // movimento suave
        Vector3 target = new Vector3(targetX, targetY, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, followSmoothTime);
    }
     public void ForceSetInHell(bool value)
{
    inHell = value;
    StopAllCoroutines();
    float targetY = inHell ? hellCameraY : (player != null ? player.position.y + groundYOffset : groundYOffset);
    transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
}
    // Chamado quando o jogador entra ou sai do inferno
    public void SetInHell(bool value)
    {
        if (value == inHell) return;
        inHell = value;

        if (transCoroutine != null)
            StopCoroutine(transCoroutine);

        transCoroutine = StartCoroutine(TransitionY(inHell ? hellCameraY : (player.position.y + groundYOffset), transitionTime));
    }

    private IEnumerator TransitionY(float targetY, float duration)
    {
        float startY = transform.position.y;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            float y = Mathf.Lerp(startY, targetY, t);
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
            yield return null;
        }
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        transCoroutine = null;
    }
}
