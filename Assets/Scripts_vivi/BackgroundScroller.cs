using UnityEngine;
[RequireComponent(typeof(Renderer))]
public class BackgroundScroller : MonoBehaviour
{
    public Vector2 speed = new Vector2(0.15f, 0f);
    Renderer rend;
    Vector2 offset;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        offset = rend.sharedMaterial.mainTextureOffset;
    }

    void Update()
    {
        offset += speed * Time.deltaTime;
        rend.sharedMaterial.mainTextureOffset = offset;
    }
}
