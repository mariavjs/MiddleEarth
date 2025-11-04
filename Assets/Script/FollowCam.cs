using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Player player;
    // Update is called once per frame
    void Update()
    {
        // Se o player ainda existe e está ativo
        if (player != null && player.gameObject.activeInHierarchy)
        {
            transform.Translate(Vector2.right * player.speed * Time.deltaTime);
        }
        // Se o player morreu ou foi destruído, a câmera para   de se mover               }
    }
}
