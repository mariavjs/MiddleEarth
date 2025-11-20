using UnityEngine;
using UnityEngine.UI;    // <- necessário para Image
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
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
    [HideInInspector]
    public int currentLives;       // vidas atuais em tempo de execução

    [Header("UI - Hearts (imagens)")]
    public Image[] heartImages;    // arraste as imagens Heart1..HeartN aqui (em ordem)
    public TextMeshProUGUI livesText; // opcional: mostra número além dos corações

    [Header("Tags / Layers")]
    [Tooltip("Se quiser que o player responda diretamente a um trigger com tag Hell, configure aqui.")]
    public string hellTag = "Hell"; // opcional: configure a tag do HellCollider se for usar OnTriggerEnter2D no Player

    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        // força travar X e permitir Y (garante que o player não ande horizontalmente)
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        animator = GetComponent<Animator>();

        if (audioSource == null)
            Debug.LogWarning($"[{name}] AudioSource não encontrado no Player. Adicione um AudioSource.");

        // garante que o número inicial de vidas esteja dentro do que podemos exibir
        int maxDisplayable = (heartImages != null) ? heartImages.Length : 0;
        if (maxDisplayable <= 0)
            currentLives = Mathf.Max(0, startingLives);
        else
            currentLives = Mathf.Clamp(startingLives, 0, maxDisplayable);

        UpdateLivesUI();
    }

    void Update()
    {
        if (isDead) return;

        // Entrada: pulo
        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            Jump();
            if (animator != null && HasAnimatorParam("Jump")) animator.SetBool("Jump", true);
            canJump = false;
        }

        // debug som
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);
        }

        // se cair mto baixo, dá dano (fallback)
        if (transform.position.y < -50f)
        {
            // valor arbitrário — ajuste ou remova
            TakeDamage(1);
        }

        // animação Run (se existir)
        if (animator != null && HasAnimatorParam("Run"))
            animator.SetBool("Run", true);
    }

    void Jump()
    {
        if (rb == null) return;
        Vector2 v = rb.linearVelocity;          // CORREÇÃO: usar rb.velocity
        v.y = jumpHeight;
        rb.linearVelocity = v;
        if (animator != null && HasAnimatorParam("Jump")) animator.SetBool("Jump", true);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
            if (animator != null && HasAnimatorParam("Jump")) animator.SetBool("Jump", false);
        }
    }

    // Opção: se preferir detectar o HellCollider diretamente no Player
    void OnTriggerEnter2D(Collider2D other)
    {
        if (string.IsNullOrEmpty(hellTag)) return;
        if (other.CompareTag(hellTag))
        {
            DieAndFall();
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
            Die();
        else
        {
            if (animator != null && HasAnimatorParam("Hurt")) animator.SetTrigger("Hurt");
        }
    }

    // Método público para iniciar a "morte" e queda para o inferno (chamado pelo PitTrigger/HellTrigger)
    public void DieAndFall()
    {
        if (isDead) return;
        isDead = true;

        // if (deathSound != null && audioSource != null)
        //     audioSource.PlayOneShot(deathSound);

        // desativa o comportamento do player
        this.enabled = false;

        // opcional: animação de morte já disparada antes (ex: animator.SetTrigger("Die"); )

        // Informar GameManager e abrir Game Over (GameManager pode salvar highscore, etc)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDeath();
        }

        // Mostrar painel de Game Over (vai pausar o jogo)
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver();
        }

        // destrói o objeto visual depois de um tempo, se desejar
        Destroy(gameObject, 0.5f);
        // não setamos Time.timeScale = 0f aqui — o GameOverManager cuida disso
    }


    // Atualiza as imagens de coração e (opcional) texto
    private void UpdateLivesUI()
    {
        if (heartImages != null && heartImages.Length > 0)
        {
            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] != null)
                    heartImages[i].gameObject.SetActive(i < currentLives);
            }
        }

        if (livesText != null) livesText.text = "Lives: " + currentLives;
    }

    // ---------- utilitários ----------
    bool HasAnimatorParam(string param)
    {
        if (animator == null) return false;
        foreach (var p in animator.parameters)
            if (p.name == param) return true;
        return false;
    }
}
