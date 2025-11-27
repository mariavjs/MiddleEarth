using System;
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

    private void Awake()
    {
        LivesConfig.Load();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        PitManager.Instance.OnPlayerBackFromHell += OnPlayerBackFromHell;

        startingLives = LivesConfig.MaxLives;

        if (audioSource == null)
            Debug.LogWarning($"[{name}] AudioSource não encontrado no Player. Adicione um AudioSource.");

        int maxDisplayable = (heartImages != null) ? heartImages.Length : 0;
        if (maxDisplayable <= 0) currentLives = Mathf.Max(0, startingLives);
        else currentLives = Mathf.Clamp(startingLives, 0, maxDisplayable);

        UpdateLivesUI();

        Debug.Log($"[Player] Start - lives = {currentLives}");
    }

    private void OnPlayerBackFromHell()
    {
        currentLives = startingLives;
        int maxDisplayable = (heartImages != null) ? heartImages.Length : 0;
        if (maxDisplayable <= 0) currentLives = Mathf.Max(0, startingLives);
        else currentLives = Mathf.Clamp(startingLives, 0, maxDisplayable);

        UpdateLivesUI();
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

        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && canJump)
        {
            Jump();
            if (animator != null) animator.SetBool("Jump", true);
            canJump = false;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);
        }
    }

    public void Jump()
    {
        if (rb == null) return;
        Vector2 v = rb.linearVelocity; // propriedade correta
        v.y = jumpHeight;
        rb.linearVelocity = v;
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
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentLives = Mathf.Max(0, currentLives - amount);
        UpdateLivesUI();

        if (hitSound != null && audioSource != null) audioSource.PlayOneShot(hitSound);

        // iniciar pit quando ficar sem vida (comportamento desejado)
        if (currentLives == 0)
        {
            {
                if (PitManager.Instance.IsPlayerInHell())
                {
                    Die();
                }
                else
                {
                    Debug.Log("[Player] currentLives == 0 -> requesting PitManager to start pit");
                    if (PitManager.Instance != null)
                        PitManager.Instance.StartPitForPlayer(this.transform);
                    else
                        Debug.LogWarning("[Player] PitManager.Instance null when requesting pit.");
                }
            }

            return;
        }

        if (animator != null) animator.SetTrigger("Hurt");
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

        GameManager.Instance?.OnPlayerDeath();
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

    public void SetStartingLives(int newStarting)
    {
        startingLives = newStarting;
        int maxDisplay = (heartImages != null) ? heartImages.Length : newStarting;
        currentLives = Mathf.Clamp(startingLives, 0, maxDisplay);
        UpdateLivesUI();
    }
}
