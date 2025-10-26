using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float acceleration = 1.2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        speed += acceleration * Time.deltaTime;
        // move o player em direção X positiva
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }
}
