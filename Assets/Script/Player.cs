using UnityEngine; 

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float acceleration = 1.2f;
    private Rigidbody2D rb;
    private Animator animator;
    public float jumpHeight = 10f;
    private bool canJump = true;

    public AudioSource deathSound;


    void Start()
{
    rb = GetComponent<Rigidbody2D>();
    animator = GetComponent<Animator>();
    if (rb == null) Debug.LogWarning("Player: Rigidbody2D faltando!");
}

    void Update()
    {
        speed += acceleration * Time.deltaTime;
        // move o player em direção X positiva
       // transform.Translate(Vector2.right * speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            // pula o player
            Jump();
            animator.SetBool("Jump", true);
            canJump = false; // impede pulo duplo
        }
    }
    void Jump()
{
    if (rb == null) rb = GetComponent<Rigidbody2D>();
    if (rb == null) return; // evita crash no editor

    Vector2 v = rb.linearVelocity;     // use 'velocity' (Rigidbody2D) 
    v.y = jumpHeight;
    rb.linearVelocity = v;
}


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // O player pode pular novamente
            canJump = true;
            animator.SetBool("Jump", false);
        }
    }

    public void Die()
    {
        // Toca som de morte se tiver um AudioSource
        AudioSource deathSound = GetComponent<AudioSource>();
        if (deathSound != null)
        {
            deathSound.Play();
        }

        // // Aqui você pode tocar animação também
        // if (animator != null)
        // {
        //     animator.SetBool("Dead", true);
        // }

        // Desativa o movimento
        this.enabled = false;

        // Destroi o player depois de um pequeno delay (pra som tocar)
        Destroy(gameObject, 0.5f);
        Time.timeScale = 0f; // pausa tudo no jogo

    }

    // void OnTriggerEnter2D(Collider2D other)
    // {
    //     // atravessou o gatilho (child "Endpoint" com Tag = "Box")
    //     if (other.CompareTag("Box"))
    //     {
    //         var spawner = FindObjectOfType<GroundSpawner>();
    //         if (spawner) spawner.SpawnTile();
    //     }
    // }
}