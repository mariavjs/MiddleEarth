using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Player player;
    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.right * player.speed * Time.deltaTime);
    }
}
