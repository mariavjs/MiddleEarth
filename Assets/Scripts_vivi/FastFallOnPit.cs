using System.Collections;
using UnityEngine;
using TMPro;

[AddComponentMenu("Game/FastFallOnPit")]
public class FastFallOnPit : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public MonoBehaviour playerController; // o script de controle do player (opcional)
    public LayerMask groundLayers;
    public CameraFollow cameraFollow;     // opcional, pra subir a câmera depois
    public PitYTrigger pitYTrigger;       // opcional, para pedir retorno ao PitYTrigger

    [Header("Ajustes de queda")]
    [Tooltip("multiplica gravityScale do Rigidbody2D (2-6 é razoável)")]
    public float gravityMultiplier = 3f;
    [Tooltip("empurrão vertical inicial (negativo). Ex: -12 ou -20")]
    public float initialDownwardVelocity = -18f;
    [Tooltip("safety timeout para a rotina de queda")]
    public float maxFallTime = 3f;

    [Header("Detecção de chão (pés)")]
    public Vector2 feetOffset = new Vector2(0f, -0.6f);
    public float feetCheckRadius = 0.15f;

    [Header("Timer do inferno (sobrevive / volta)")]
    [Tooltip("Tempo (s) até o jogador voltar à terra se ainda estiver vivo")]
    public float hellDuration = 7f;

    [Header("UI (opcional)")]
    public TextMeshProUGUI hellTimerText;

    [Header("UI Effects (flash/pulse)")]
    public float flashThreshold = 2f;
    public Color normalColor = Color.white;
    public Color flashColor = Color.red;
    [Tooltip("Escala máxima do pulsar (1 = sem escala)")]
    public float pulseMaxScale = 1.4f;
    [Tooltip("Velocidade do piscar/pulsar")]
    public float pulseSpeed = 6f;

    // runtime
    private Rigidbody2D rb;
    private float originalGravity;
    private Coroutine fallCoroutine;
    private Coroutine hellCoroutine;
    private Player playerScript;

    void Start()
    {

        if (hellTimerText != null) {
        hellTimerText.gameObject.SetActive(true);
        hellTimerText.text = "TESTE UI";
        hellTimerText.color = Color.white;
        hellTimerText.fontSize = 48;
    }

        if (player == null)
        {
            Debug.LogWarning("FastFallOnPit: player não atribuído no Inspector.");
            return;
        }

        rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogWarning("FastFallOnPit: Rigidbody2  não encontrado no player.");

        playerScript = player.GetComponent<Player>();

        // initialize UI hidden if present
        if (hellTimerText != null)
        {
            hellTimerText.gameObject.SetActive(false);
            hellTimerText.text = "";
        }
    }

    // chamada pública para iniciar a queda rápida + timer do inferno
    public void StartFastFall()
    {
        if (player == null || rb == null)
        {
            Debug.LogWarning("FastFallOnPit: player ou Rigidbody2D ausente; abortando StartFastFall.");
            return;
        }

        // cancela rotinas anteriores
        if (fallCoroutine != null) StopCoroutine(fallCoroutine);
        if (hellCoroutine != null) StopCoroutine(hellCoroutine);

        fallCoroutine = StartCoroutine(FastFallCoroutine());
        hellCoroutine = StartCoroutine(HellTimerCoroutine());
    }

    private IEnumerator FastFallCoroutine()
    {
        // desabilita controle do player (se fornecido)
        if (playerController != null) playerController.enabled = false;

        // salva e aplica gravidade aumentada
        originalGravity = rb.gravityScale;
        rb.gravityScale = originalGravity * gravityMultiplier;

        // aplica empurrão inicial vertical
        if (!Mathf.Approximately(initialDownwardVelocity, 0f))
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, initialDownwardVelocity);

        float elapsed = 0f;
        bool grounded = false;

        while (elapsed < maxFallTime)
        {
            if (IsPlayerGrounded())
            {
                grounded = true;
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // restaura gravidade e reabilita controle
        rb.gravityScale = originalGravity;
        if (playerController != null) playerController.enabled = true;

        fallCoroutine = null;

        // se pousou, cancelamos o timer do inferno para evitar retorno automático
        if (grounded && hellCoroutine != null)
        {
            StopCoroutine(hellCoroutine);
            hellCoroutine = null;

            if (hellTimerText != null)
            {
                hellTimerText.text = "";
                hellTimerText.gameObject.SetActive(false);
                hellTimerText.color = normalColor;
                hellTimerText.rectTransform.localScale = Vector3.one;
            }
        }
    }

    private IEnumerator HellTimerCoroutine()
    {
        float timer = 0f;

        // ativa UI se existir
        if (hellTimerText != null)
        {
            hellTimerText.gameObject.SetActive(true);
            hellTimerText.color = normalColor;
            hellTimerText.rectTransform.localScale = Vector3.one;
        }

        while (timer < hellDuration)
        {
            // se o player morreu (vidas = 0) cancela e limpa UI
            if (playerScript == null || playerScript.currentLives <= 0)
            {
                if (hellTimerText != null)
                {
                    hellTimerText.text = "";
                    hellTimerText.gameObject.SetActive(false);
                    hellTimerText.rectTransform.localScale = Vector3.one;
                    hellTimerText.color = normalColor;
                }
                yield break;
            }

            float remaining = Mathf.Max(0f, hellDuration - timer);

            // atualiza texto (1 casa decimal)
            if (hellTimerText != null)
                hellTimerText.text = $"Tempo para sobreviver: {remaining:F1}s";

            // efeito piscar/pulsar nos últimos flashThreshold segundos
            if (hellTimerText != null && remaining <= flashThreshold)
            {
                float ping = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                hellTimerText.color = Color.Lerp(normalColor, flashColor, ping);

                float scaleT = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                float s = Mathf.Lerp(1f, pulseMaxScale, scaleT);
                hellTimerText.rectTransform.localScale = Vector3.one * s;
            }
            else if (hellTimerText != null)
            {
                hellTimerText.color = normalColor;
                hellTimerText.rectTransform.localScale = Vector3.one;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // tempo esgotado -> jogador sobreviveu? se sim, volta pra terra
        if (hellTimerText != null)
        {
            hellTimerText.text = "";
            hellTimerText.gameObject.SetActive(false);
            hellTimerText.rectTransform.localScale = Vector3.one;
            hellTimerText.color = normalColor;
        }

        if (playerScript != null && playerScript.currentLives > 0)
        {
            // pede para a camera voltar e para o PitYTrigger resetar o estado
            if (cameraFollow != null) cameraFollow.SetInHell(false);
            if (pitYTrigger != null) pitYTrigger.ForceReturnToGround();

            // reposiciona o player para a superfície (ajuste aqui se necessário)
            Vector3 newPos = player.position;
            newPos.y = 0f; // ajuste para a Y da surface no seu nível
            player.position = newPos;
        }

        hellCoroutine = null;
    }

    private bool IsPlayerGrounded()
    {
        if (player == null) return false;
        Vector2 feetPos = (Vector2)player.position + feetOffset;
        Collider2D hit = Physics2D.OverlapCircle(feetPos, feetCheckRadius, groundLayers);
        return hit != null;
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.yellow;
        Vector2 feetPos = (Vector2)player.position + feetOffset;
        Gizmos.DrawWireSphere(feetPos, feetCheckRadius);
    }
}
