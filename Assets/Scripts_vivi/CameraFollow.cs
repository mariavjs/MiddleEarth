using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float followSmoothTime = 0.12f;   // suavidade seguindo X do player
    public float transitionTime = 0.25f;        // tempo da transição Y
    public float groundYOffset = 0f;         // offset Y quando em terra relativo ao player
    public float hellCameraY = -10f;         // posição Y fixa da câmera no inferno (world Y)
    public bool keepXFollowing = true;

    private Vector3 velocity = Vector3.zero;
    private bool inHell = false;
    private Coroutine transCoroutine;

    void LateUpdate()
    {
        if (player == null) return;

        // Segue X do player sempre (opcional)
        float targetX = keepXFollowing ? player.position.x : transform.position.x;

        if (!inHell)
        {
            float targetY = player.position.y + groundYOffset;
            Vector3 target = new Vector3(targetX, targetY, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, followSmoothTime);
        }
        // se inHell, mantemos posição Y fixa (transCoroutine gerencia a transição)
        else
        {
            Vector3 target = new Vector3(targetX, hellCameraY, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, followSmoothTime);
        }
    }

    // Chamado por PitYTrigger
    public void SetInHell(bool value)
    {
        if (value == inHell) return;
        inHell = value;

        // Para a transição suave, interrompe a anterior e inicia nova
        if (transCoroutine != null) StopCoroutine(transCoroutine);
        transCoroutine = StartCoroutine(TransitionY(value ? hellCameraY : (player.position.y + groundYOffset), transitionTime));
    }

    private IEnumerator TransitionY(float targetY, float duration)
    {
        float startY = transform.position.y;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float y = Mathf.Lerp(startY, targetY, Mathf.SmoothStep(0f,1f,t));
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // garantir valor final
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        transCoroutine = null;
    }
}
