using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 5f;          // velocidade horizontal inicial
    public float acceleration = 1.2f; // aceleração contínua

    [Header("Pulo")]
    public float jumpHeight = 10f;    // velocidade vertical do pulo

    private Rigidbody2D rb;
    private Animator animator;
    private bool canJump = true;
    private float currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentSpeed = speed; // começa com a speed inicial
    }

    void Update()
    {
        // acelera ao longo do tempo
        currentSpeed += acceleration * Time.deltaTime;

        // entrada de pulo
        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            Jump();
            if (animator) animator.SetBool("Jump", true);
            canJump = false; // evita pulo duplo
        }
    }

    void FixedUpdate()
    {
        // movimento horizontal via física (correto para Rigidbody2D)
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        // >>> CORREÇÃO: usar rb.velocity (não linearVelocity)
        Vector2 v = rb.linearVelocity;
        v.y = jumpHeight;
        rb.linearVelocity = v;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // aterrissou no chão sólido
        if (collision.collider.CompareTag("Ground"))
        {
            canJump = true;
            if (animator) animator.SetBool("Jump", false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // atravessou o gatilho (child "Endpoint" com Tag = "Box")
        if (other.CompareTag("Box"))
        {
            var spawner = FindObjectOfType<GroundSpawner>();
            if (spawner) spawner.SpawnTile();
        }
    }
}
