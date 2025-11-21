using UnityEngine;

public class ParallaxMove : MonoBehaviour
{
    [Header("Configuração do movimento")]
    public float speed = 1f;      // Velocidade do movimento
    public float width = 20f;     // Largura do sprite (em unidades)

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // move o fundo pra esquerda
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // quando o fundo sair da tela, volta pra posição inicial (loop)
        if (transform.position.x < startPos.x - width)
        {
            transform.position = startPos;
        }
    }
}
