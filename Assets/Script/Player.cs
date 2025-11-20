using UnityEngine;
using UnityEngine.UI;    // <- necessário para Image
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
    public AudioClip hitSound;
    public AudioClip deathSound;
    private AudioSource audioSource;

    [Tooltip("Quantas vidas o jogador começa tendo (ex: 3).")]
    public int startingLives = 3;   // quantidade inicial (padrão 3)
    public int currentLives;       // vidas atuais em tempo de execução

    [Header("UI - Hearts (imagens)")]
    public Image[] heartImages;    // arraste as imagens Heart1..HeartN aqui (em ordem)
    public TextMeshProUGUI livesText; // opcional: mostra número além dos corações

    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();


        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogWarning($"[{name}] AudioSource não encontrado no Player. Adicione um AudioSource.");

        // garante que o número inicial de vidas esteja dentro do que podemos exibir
        int maxDisplayable = (heartImages != null) ? heartImages.Length : 0;
        if (maxDisplayable <= 0)
        {
            // se não houver imagens, usamos startingLives diretamente
            currentLives = Mathf.Max(0, startingLives);
        }
        else
        {
            // se houver N corações na UI, começa com min(startingLives, N)
            currentLives = Mathf.Clamp(startingLives, 0, maxDisplayable);
        }


        UpdateLivesUI();
    }

    void Update()
    {
        if (isDead) return;

        // speed += acceleration * Time.deltaTime;

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

        if (transform.position.y < -15f)
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
            if (animator != null) animator.SetBool("Jump", false);
        }
    }

    // Aplica dano; garante que não fique negativo
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentLives = Mathf.Max(0, currentLives - amount);
        UpdateLivesUI();


        if (hitSound != null && audioSource != null)

            audioSource.PlayOneShot(hitSound);


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

            audioSource.PlayOneShot(deathSound);


        this.enabled = false;


        Destroy(gameObject, 0.5f);
        Time.timeScale = 0f;
    }

    // Atualiza as imagens de coração e (opcional) texto
    private void UpdateLivesUI()
    {
        // 1) Se houver imagens (heartImages), ativa/desativa conforme currentLives
        if (heartImages != null && heartImages.Length > 0)
        {
            for (int i = 0; i < heartImages.Length; i++)
            {
                // Exibe o coração se o índice for menor que currentLives
                if (heartImages[i] != null)
                    heartImages[i].gameObject.SetActive(i < currentLives);
            }
        }

        // 2) Se livesText estiver configurado, mantenha como fallback (opcional)
        if (livesText != null)
        {
            livesText.text = "Lives: " + currentLives;
        }
    }

    // Método para adicionar vidas (ex: comprar ou ganhar vida)
    // Retorna quantas vidas realmente foram adicionadas
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
        return currentLives - prev; // quantidade efetivamente adicionada
    }

    // Método para definir um novo startingLives (usado quando o jogador compra vidas extras no shop)
    // OBS: se o player pode ter mais corações visíveis, você precisa adicionar imagens no UI e aumentar heartImages length.
    public void SetStartingLives(int newStarting)
    {
        startingLives = newStarting;
        // atualiza currentLives respeitando o máximo exibível
        int maxDisplay = (heartImages != null) ? heartImages.Length : newStarting;
        currentLives = Mathf.Clamp(startingLives, 0, maxDisplay);
        UpdateLivesUI();
    }
}