using UnityEngine;

[RequireComponent(typeof(Transform))]
public class ParallaxLoopTwoSprites : MonoBehaviour
{
    [Tooltip("Left sprite (de preferência os dois com o mesmo SpriteRenderer e mesma escala).")]
    public Transform leftSprite;
    [Tooltip("Right sprite (colocado à direita do leftSprite).")]
    public Transform rightSprite;

    [Tooltip("Velocidade em unidades/segundo. Valores positivos movem para a esquerda.")]
    public float speed = 2f;

    private float spriteWidth = 0f;

    void Start()
    {
        if (leftSprite == null || rightSprite == null)
        {
            Debug.LogError("[ParallaxLoopTwoSprites] atribua Left e Right no Inspector.");
            enabled = false;
            return;
        }

        // calcula largura (world units) a partir do SpriteRenderer bounds (usa o left)
        var sr = leftSprite.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            spriteWidth = sr.bounds.size.x * leftSprite.lossyScale.x;
        }
        else
        {
            // fallback: tenta baseado no transform local (se for quad com tamanho diferente)
            spriteWidth = Mathf.Abs(rightSprite.position.x - leftSprite.position.x);
            if (spriteWidth <= 0f) spriteWidth = 10f;
        }
    }

    void Update()
    {
        float move = speed * Time.deltaTime;
        leftSprite.position += Vector3.left * move;
        rightSprite.position += Vector3.left * move;

        // se o left saiu totalmente à esquerda, reposiciona ele à direita do right
        if (leftSprite.position.x + (spriteWidth * 0.5f) < Camera.main.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect)
        {
            // move leftSprite para a direita do rightSprite
            leftSprite.position = new Vector3(rightSprite.position.x + spriteWidth, leftSprite.position.y, leftSprite.position.z);
            // swap referencias pra manter lógica (o que antes era right vira left)
            var tmp = leftSprite;
            leftSprite = rightSprite;
            rightSprite = tmp;
        }
        // (não precisa checar o right, swap cuida da continuidade)
    }
}
