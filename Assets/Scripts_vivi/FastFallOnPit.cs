using System.Collections;
using UnityEngine;

[AddComponentMenu("Game/FastFallOnPit")]
public class FastFallOnPit : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;                   // arraste o Transform do player
    public MonoBehaviour playerController;     // arraste aqui o script de controle do player (ex: Player, PlayerController)
    public LayerMask groundLayers;             // camada(s) de chão (inferno)
    
    [Header("Ajustes de queda")]
    public float gravityMultiplier = 3f;       // multiplica gravityScale do Rigidbody2D
    public float initialDownwardVelocity = -8f;// valor inicial y (negativo) aplicado quando começar a queda; 0 = nenhum push
    public float maxFallTime = 3f;             // safety timeout
    public Vector2 feetOffset = new Vector2(0f, -0.6f); // offset relativo ao player para checar o chão
    public float feetCheckRadius = 0.15f;      // radius do overlap circle

    // runtime
    private Rigidbody2D rb;
    private float originalGravity;
    private Coroutine currentCoroutine;

    void Reset()
    {
        // defaults úteis
        gravityMultiplier = 3f;
        initialDownwardVelocity = -8f;
        maxFallTime = 3f;
        feetOffset = new Vector2(0f, -0.6f);
        feetCheckRadius = 0.15f;
    }

    void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("FastFallOnPit: player não atribuído no Inspector.");
            return;
        }
        rb = player.GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogWarning("FastFallOnPit: Rigidbody2D não encontrado no player.");
    }

    public void StartFastFall()
    {
        if (player == null || rb == null)
        {
            Debug.LogWarning("FastFallOnPit: player ou Rigidbody2D faltando; abortando StartFastFall.");
            return;
        }

        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(FastFallCoroutine());
    }

    private IEnumerator FastFallCoroutine()
    {
        // disable player control
        if (playerController != null) playerController.enabled = false;

        originalGravity = rb.gravityScale;
        rb.gravityScale = originalGravity * gravityMultiplier;

        if (!Mathf.Approximately(initialDownwardVelocity, 0f))
        {
            // força a velocidade vertical negativa
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, initialDownwardVelocity);
        }

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

        // garantia: se timeout, deixamos o player cair mas restauramos gravidade
        rb.gravityScale = originalGravity;

        // reativa controle do player
        if (playerController != null) playerController.enabled = true;

        currentCoroutine = null;
    }

    private bool IsPlayerGrounded()
    {
        Vector2 feetPos = (Vector2)player.position + feetOffset;
        Collider2D hit = Physics2D.OverlapCircle(feetPos, feetCheckRadius, groundLayers);
        return hit != null;
    }

    // utilitário para visualizar o círculo no Editor
    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.yellow;
        Vector2 feetPos = (Vector2)player.position + feetOffset;
        Gizmos.DrawWireSphere(feetPos, feetCheckRadius);
    }
}
