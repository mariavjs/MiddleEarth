using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FastFallOnPit : MonoBehaviour
{
    Rigidbody2D rb;

    [Header("Ajustes Fast Fall")]
    public float fastGravityMultiplier = 5f;   // multiplica a gravidade
    public float initialDownVelocity = -8f;    // aplica um impulso inicial

    float originalGravityScale;
    bool isFast = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogError("[FastFallOnPit] Rigidbody2D não encontrado!");
        originalGravityScale = rb != null ? rb.gravityScale : 1f;
    }

    public void StartFastFall()
    {
        if (rb == null) return;
        if (isFast) return;
        isFast = true;
        rb.gravityScale = originalGravityScale * fastGravityMultiplier;
        Vector2 v = rb.linearVelocity;
        v.y = initialDownVelocity;
        rb.linearVelocity = v;
    }

    public void StopFastFall()
    {
        if (rb == null) return;
        if (!isFast) return;
        isFast = false;
        rb.gravityScale = originalGravityScale;
    }
}
