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
    public AudioClip hitSound;   // som quando leva dano
    public AudioClip deathSound; // som quando morre
    private AudioSource audioSource;

    public int maxLives = 3;
    public int currentLives;
    public TextMeshProUGUI livesText;

    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // <-- A linha que faltava: pega o AudioSource anexado ao Player
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogWarning($"[{name}] AudioSource não encontrado no Player. Adicione um AudioSource ao GameObject.");

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
            if (animator != null) animator.SetBool("Jump", true);
            canJump = false;
        }

        // teste rápido: tocar som manualmente com K (útil pra debug)
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);
        }

        if (transform.position.y < -10f)
        {
            TakeDamage(1);
        }
    }

    void Jump()
    {
        if (rb == null) return;

        Vector2 v = rb.linearVelocity; // uso correto
        v.y = jumpHeight;
        rb.linearVelocity = v;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
            if (animator != null) animator.SetBool("Jump", false);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentLives -= amount;
        UpdateLivesUI();

        // toca som de hit (imediato)
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        if (currentLives <= 0)
        {
            Die();
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

        // toca som de morte (toca e esperamos antes de destruir)
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        this.enabled = false;

        // espera um tempo curto pra som tocar e então destrói (0.5s é um bom começo)
        Destroy(gameObject, 0.5f);
        Time.timeScale = 0f;
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + currentLives;
        }
    }
}
