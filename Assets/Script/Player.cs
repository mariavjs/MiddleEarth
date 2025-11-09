using UnityEngine; 
using TMPro; 

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
    public AudioSource deathSound;
    public int maxLives = 3;
    public int currentLives;
    public TextMeshProUGUI livesText; // arraste o texto de UI aqui (opcional)

    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentLives = maxLives;
        UpdateLivesUI();
    }

    void Update()
    {
        if (isDead) return;

        speed += acceleration * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            Jump();
            animator.SetBool("Jump", true);
            canJump = false;
        }

        // 🧠 Verifica se caiu da tela (ex: y < -10)
        if (transform.position.y < -10f)
        {
            TakeDamage(1);
        }
    }

    void Jump()
    {
        if (rb == null) return;

        Vector2 v = rb.linearVelocity;
        v.y = jumpHeight;
        rb.linearVelocity = v;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
            animator.SetBool("Jump", false);
        }
    }

    // 🩸 Player toma dano
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentLives -= amount;
        UpdateLivesUI();

        if (currentLives <= 0)
        {
            Die();
        }
        else
        {
            // opcional: animação de dano
            if (animator != null)
                animator.SetTrigger("Hurt");
        }
    }

    // ☠️ Player morre
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (deathSound != null)
            deathSound.Play();

        this.enabled = false;

        // Destroi o player depois de um pequeno delay (pra som tocar)
        Destroy(gameObject, 0.5f);
        Time.timeScale = 0f; // pausa tudo no jogo
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + currentLives;
        }
    }
}