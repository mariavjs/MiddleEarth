using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float acceleration = 1.2f;
    private Rigidbody2D rb;
    private Animator animator;

    public float jumpHeight = 10f; 
    private bool canJump = true; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
{
    rb = GetComponent<Rigidbody2D>();
    animator = GetComponent<Animator>();
    if (rb == null) Debug.LogWarning("Player: Rigidbody2D faltando!");
}

    // Update is called once per frame
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
        if (collision.gameObject.CompareTag("Ground")) // if collision.collider.tag == "Ground"
        {
            // O player pode pular novamente
            canJump = true;
            animator.SetBool("Jump", false);

            // Debug.Log("Player landed on the ground.");
        }
    }
}
