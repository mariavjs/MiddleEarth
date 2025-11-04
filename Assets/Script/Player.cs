using UnityEngine; 

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float acceleration = 1.2f;
    private Rigidbody2D rb;
    private Animator animator;
    public float jumpHeight = 10f;
    private bool canJump = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        speed += acceleration * Time.deltaTime;
        // move o player em direção X positiva
        transform.Translate(Vector2.right * speed * Time.deltaTime);

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
        Vector2 velocity = rb.linearVelocity;
        velocity.y = jumpHeight;
        rb.linearVelocity = velocity;
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
}