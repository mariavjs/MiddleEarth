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

    [Header("Fixar na tela")]
    [Tooltip("Posição em viewport onde o player ficará (x: 0..1, y usado só para referência)")]
    public Vector2 viewportPos = new Vector2(0.25f, 0.5f);
    private Camera mainCam;
    private float camToPlayerDistance = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Recomendo: Freeze Rotation apenas, controle X via MovePosition no FixedUpdate
        if (rb != null)
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogWarning($"[{name}] AudioSource não encontrado no Player. Adicione um AudioSource ao GameObject.");

        currentLives = maxLives;
        UpdateLivesUI();

        mainCam = Camera.main;
        if (mainCam != null)
        {
            // distância entre câmera e o plano do player (normalmente cam.z = -10, player.z = 0 => 10)
            camToPlayerDistance = Mathf.Abs(mainCam.transform.position.z - transform.position.z);
        }
    }

    void Update()
    {
        if (isDead) return;

        // speed += acceleration * Time.deltaTime; // removido, se não usar movimento horizontal

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            Jump();
            if (animator != null) animator.SetBool("Jump", true);
            canJump = false;
        }

        // força animação de corrida (para parecer que corre parado)
        if (animator != null)
        {
            animator.SetBool("Run", true);
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

    // Usamos FixedUpdate para posicionar X via MovePosition (compatível com física)
    void FixedUpdate()
    {
        if (rb == null || mainCam == null) return;

        // calcula o X em world correspondente ao viewportPos.x
        Vector3 vp = new Vector3(viewportPos.x, viewportPos.y, camToPlayerDistance);
        Vector3 worldPoint = mainCam.ViewportToWorldPoint(vp);

        // mantemos a Y física (rb.position.y) e z original
        Vector2 target = new Vector2(worldPoint.x, rb.position.y);

        // MovePosition respeita a física e é suave em FixedUpdate
        rb.MovePosition(target);
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

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        this.enabled = false;
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
