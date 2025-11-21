using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 5f;
    public float acceleration = 1.2f;
    private Rigidbody2D rb;
    private Animator animator;
    public float jumpHeight = 10f;
    private bool canJump = true;

    [Header("Som e Vidas")]
    public AudioClip hitSound;
    public AudioClip deathSound;
    private AudioSource audioSource;

    [Tooltip("Quantas vidas o jogador começa tendo (ex: 3).")]
    public int startingLives = 3;
    public int currentLives;

    [Header("UI - Hearts (imagens)")]
    public Image[] heartImages;
    public TextMeshProUGUI livesText;

    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            Debug.LogWarning($"[{name}] AudioSource não encontrado no Player. Adicione um AudioSource.");

        int maxDisplayable = (heartImages != null) ? heartImages.Length : 0;
        if (maxDisplayable <= 0) currentLives = Mathf.Max(0, startingLives);
        else currentLives = Mathf.Clamp(startingLives, 0, maxDisplayable);

        UpdateLivesUI();

        Debug.Log($"[Player] Start - lives = {currentLives}");
    }

    void Update()
    {
        if (isDead) return;

        // DEBUG: tecla H força pit (útil para testar)
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("[Player] Debug H pressed -> forcing pit start");
            if (PitManager.Instance != null) PitManager.Instance.StartPitForPlayer(transform);
            else Debug.LogWarning("[Player] PitManager.Instance null when forcing pit.");
        }

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            Jump();
            if (animator != null) animator.SetBool("Jump", true);
            canJump = false;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);
        }

        // exemplo de fallback: se cair muito, perde 1 vida
        if (transform.position.y < -10f)
        {
            Debug.Log("[Player] fell below -10 -> TakeDamage(1)");
            TakeDamage(1);
        }
    }

    public void Jump()
    {
        if (rb == null) return;
        Vector2 v = rb.linearVelocity; // propriedade correta
        v.y = jumpHeight;
        rb.linearVelocity = v;

        Debug.Log($"[Player] Jump -> velocity.y = {rb.linearVelocity.y}");
    }

    public void ForceAllowJump()
    {
        canJump = true;
        if (animator != null) animator.SetBool("Jump", false);
        Debug.Log("ForceAllowJump() chamado -> canJump = true");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
            if (animator != null) animator.SetBool("Jump", false);
            Debug.Log("Ground detected -> canJump = true");
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
        }
    }

    // ÚNICA definição de OnTriggerEnter2D (remova outras)
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Player] OnTriggerEnter2D with '{other.gameObject.name}' tag={other.gameObject.tag}");

        if (other.CompareTag("Enemy"))
        {
            Debug.Log("[Player] Hit Enemy -> calling TakeDamage(1)");
            TakeDamage(1);

            // Se preferir: ir direto ao inferno ao tocar inimigo (independente de vidas)
            // PitManager.Instance?.StartPitForPlayer(this.transform);
        }

        // se usar pit trigger separado:
        if (other.CompareTag("PitTrigger"))
        {
            Debug.Log("[Player] Entered PitTrigger -> starting pit");
            PitManager.Instance?.StartPitForPlayer(this.transform);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentLives = Mathf.Max(0, currentLives - amount);
        UpdateLivesUI();

        if (hitSound != null && audioSource != null) audioSource.PlayOneShot(hitSound);

        // iniciar pit quando ficar com 1 vida (comportamento desejado)
        if (currentLives == 1)
        {
            Debug.Log("[Player] currentLives == 1 -> requesting PitManager to start pit");
            if (PitManager.Instance != null)
                PitManager.Instance.StartPitForPlayer(this.transform);
            else
                Debug.LogWarning("[Player] PitManager.Instance null when requesting pit.");
        }

        if (currentLives <= 0)
        {
            Die(); // chama Die uma vez (sem recursão)
        }
        else
        {
            if (animator != null) animator.SetTrigger("Hurt");
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[Player] Die() called");

        if (deathSound != null && audioSource != null) audioSource.PlayOneShot(deathSound);

        if (animator != null) animator.SetTrigger("Die");

        // desativa input do player; não destrua o objeto enquanto estiver testando pit/timer
        this.enabled = false;

        // Se desejar, chame GameOverManager aqui (comente se quiser testar timer)
        // GameOverManager.Instance?.ShowGameOver();
    }

    private void UpdateLivesUI()
    {
        if (heartImages != null && heartImages.Length > 0)
        {
            for (int i = 0; i < heartImages.Length; i++)
                if (heartImages[i] != null)
                    heartImages[i].gameObject.SetActive(i < currentLives);
        }

        if (livesText != null) livesText.text = "Lives: " + currentLives;
    }

    public int AddLife(int amount = 1)
    {
        if (isDead) return 0;

        if (heartImages == null || heartImages.Length == 0)
        {
            currentLives += amount;
            UpdateLivesUI();
            return amount;
        }

        int prev = currentLives;
        currentLives = Mathf.Clamp(currentLives + amount, 0, heartImages.Length);
        UpdateLivesUI();
        return currentLives - prev;
    }

    public void SetStartingLives(int newStarting)
    {
        startingLives = newStarting;
        int maxDisplay = (heartImages != null) ? heartImages.Length : newStarting;
        currentLives = Mathf.Clamp(startingLives, 0, maxDisplay);
        UpdateLivesUI();
    }
}
