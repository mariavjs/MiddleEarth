using System;
using UnityEngine;

public class InfiniteScrolling : MonoBehaviour
{
    [SerializeField] private GameObject camera;
    [SerializeField] private Transform[] tiles;

    private float startPos;
    private float length;
    private float speed = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position.x;
        length = GetComponent<Renderer>().bounds.size.x;
    }

    // void Update()
    // {
    //     // move them left
    //     foreach (var t in tiles)
    //         t.position += Vector3.left * speed * Time.deltaTime;
    //
    //     // wrap tiles that go off screen
    //     for (int i = 0; i < tiles.Length; i++)
    //     {
    //         var t = tiles[i];
    //         // if it moved far enough left, send it to the right
    //         if (t.position.x <= tiles[0].position.x - length)
    //         {
    //             // find the tile currently farthest to the right
    //             float maxX = float.MinValue;
    //             foreach (var other in tiles)
    //                 if (other.position.x > maxX) maxX = other.position.x;
    //
    //             t.position = new Vector2(maxX + length, t.position.y);
    //         }
    //     }
    // }

    private void FixedUpdate()
    {
        float distance = GameManager.Instance.GetDistance() * 0.25f;
    
        transform.position = new Vector2(startPos - distance, transform.position.y);
    
        if (distance > startPos + length)
        {
            startPos += length;
        }
        else if (distance < startPos - length)
        {
            startPos -= length;
        }
    }
}
