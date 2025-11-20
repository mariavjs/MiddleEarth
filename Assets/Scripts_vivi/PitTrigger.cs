using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class PitTrigger : MonoBehaviour
{
    [Header("Referências")]
    public CameraFollow cameraFollow;      // arraste Main Camera (CameraFollow) aqui
    public TextMeshProUGUI timerText;      // texto opcional
    [Header("Ajustes")]
    public float surviveTime = 7f;         // tempo para poder "voltar"
    public float dieAfter = 999f;          // tempo para matar (opcional)
    public bool allowReturn = true;

    // estado
    bool playerInPit = false;
    float timer = 0f;
    Coroutine pitCoroutine;
    Transform playerTransform;
    FastFallOnPit currentFastFall; // referência para parar depois

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (playerInPit) return;

        playerTransform = other.transform;
        playerInPit = true;
        timer = 0f;

        if (cameraFollow != null) cameraFollow.SetInHell(true);

        // pega o FastFallOnPit no Player (se existir)
        currentFastFall = other.GetComponent<FastFallOnPit>();
        if (currentFastFall != null) currentFastFall.StartFastFall();

        // inicia coroutine do pit
        pitCoroutine = StartCoroutine(PitTimerCoroutine());

        Debug.Log("[PitTrigger] Player entrou no pit.");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!playerInPit) return;
        if (other.transform != playerTransform) return;

        // saiu do pit (por exemplo voltou pra plataforma)
        playerInPit = false;
        if (pitCoroutine != null) StopCoroutine(pitCoroutine);
        pitCoroutine = null;

        if (cameraFollow != null) cameraFollow.SetInHell(false);

        if (currentFastFall != null) currentFastFall.StopFastFall();
        currentFastFall = null;
        playerTransform = null;

        if (timerText != null) timerText.gameObject.SetActive(false);

        Debug.Log("[PitTrigger] Player saiu do pit.");
    }

    IEnumerator PitTimerCoroutine()
    {
        if (timerText != null) timerText.gameObject.SetActive(true);

        while (playerInPit)
        {
            timer += Time.deltaTime;

            if (timerText != null)
                timerText.text = $"Tempo: {Mathf.Max(0f, surviveTime - timer):0.0}s";

            if (timer >= dieAfter)
            {
                var playerComp = playerTransform.GetComponent<Player>();
                if (playerComp != null) playerComp.TakeDamage(playerComp.currentLives); // mata
                yield break;
            }

            if (allowReturn && timer >= surviveTime)
            {
                // sobreviveu -> volta pra terra
                if (currentFastFall != null) currentFastFall.StopFastFall();
                if (cameraFollow != null) cameraFollow.SetInHell(false);

                if (timerText != null) timerText.gameObject.SetActive(false);

                playerInPit = false;
                pitCoroutine = null;
                Debug.Log("[PitTrigger] Player sobreviveu e voltou.");
                yield break;
            }

            yield return null;
        }
    }
}
