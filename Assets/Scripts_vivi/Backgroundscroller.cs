using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class BackgroundScroller : MonoBehaviour
{
    public float scrollSpeed = 0.2f; // ajusta no Inspector
    private Renderer rend;
    private Vector2 offset;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        offset = rend.material.mainTextureOffset;
        
        // Garante que o background fique atrás do ground
        // Se for SpriteRenderer, configura o sorting order
        if (rend is SpriteRenderer spriteRenderer)
        {
            spriteRenderer.sortingOrder = -10; // Valor negativo para ficar atrás
        }
        else
        {
            // Se for MeshRenderer (quad 3D), move para trás no eixo Z
            Vector3 pos = transform.position;
            pos.z = -10f; // Tenta Z negativo
            transform.position = pos;
        }
        }
    

    void Update()
    {
        offset.x += scrollSpeed * Time.deltaTime;
        rend.material.mainTextureOffset = offset;
    }
}
