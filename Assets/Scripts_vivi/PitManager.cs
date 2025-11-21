using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Gerencia o "Pit" (Inferno). Pode ser chamado por TakeDamage do Player,
/// por um HellCollider ou qualquer outro evento.
/// Coloque este script num GameObject da cena (ex: "GameManager") e configure referências no Inspector.
/// </summary>
public class PitManager : MonoBehaviour
{
    public static PitManager Instance { get; private set; }

    [Header("Config")]
    public float surviveTime = 20f;           // tempo no inferno
    public float infernoY = -12f;            // Y destino do inferno (ajuste conforme sua cena)
    public float returnYOffset = 0.5f;       // offset ao retornar à terra
    public bool enableFastFallOnEnter = true;

    [Header("Referências")]
    public CameraFollow cameraFollow;        // arraste a Main Camera aqui
    public TextMeshProUGUI timerText;        // UI opcional (arraste)

    // estado
    Coroutine pitCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Inicia o pit para um player (teleporta -> start fastfall -> timer).
    /// </summary>
    public void StartPitForPlayer(Transform player)
    {
        if (player == null) return;
        // previne múltiplos pits simultâneos
        if (pitCoroutine != null) StopCoroutine(pitCoroutine);
        pitCoroutine = StartCoroutine(PitSequence(player));
    }
    

    IEnumerator PitSequence(Transform player)
    {
        // guarda estado
        Vector3 savedPos = player.position;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        FastFallOnPit ff = player.GetComponent<FastFallOnPit>();
        Player playerComp = player.GetComponent<Player>();

        // teleport para inferno (mantém X)
        Vector3 infernoPos = new Vector3(player.position.x, infernoY, player.position.z);
        player.position = infernoPos;

        // zera velocidade
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // camera para inferno
        if (cameraFollow != null) cameraFollow.SetInHell(true);

        // fast fall
        if (enableFastFallOnEnter && ff != null) ff.StartFastFall();

        // mostra timer
        if (timerText != null) { timerText.gameObject.SetActive(true); UpdateTimerText(surviveTime); }

        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime;
            if (timerText != null) UpdateTimerText(Mathf.Max(0f, surviveTime - timer));

            // se player foi destruído -> aborta
            if (player == null) break;

            // se o Player morreu via Player script (zero vidas), termina sem voltar
            if (playerComp != null && playerComp.currentLives <= 0)
            {
                Debug.Log("[PitManager] Player morreu no inferno.");
                break;
            }

            if (timer >= surviveTime)
            {
                // sobreviveu -> volta
                Vector3 back = savedPos;
                back.y += returnYOffset;
                player.position = back;
                if (rb != null) { rb.linearVelocity = Vector2.zero; }
                Debug.Log("[PitManager] Player sobreviveu e voltou da Hell.");
                break;
            }

            yield return null;
        }

        // cleanup: stop fastfall, hide UI, camera back
        if (ff != null) ff.StopFastFall();
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (cameraFollow != null) cameraFollow.ForceSetInHell(true);
        //var playerComp = player.GetComponent<Player>();
        if (playerComp != null) playerComp.ForceAllowJump();


        pitCoroutine = null;
    }
   
    void UpdateTimerText(float remain)
    {
        if (timerText == null) return;
        timerText.text = $"Tempo no inferno: {remain:0.0}s";
    }
}
