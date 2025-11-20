using UnityEngine;

[AddComponentMenu("Game/PitYTrigger")]
public class PitYTrigger : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public CameraFollow cameraFollow;
    public FastFallOnPit fastFallOnPit;
    public GameObject hellBackground;
    public GameObject groundBackground;

    [Header("Ajustes")]
    public float hellThresholdY = -4f;
    public bool allowReturn = true;
    public float cooldownSeconds = 0.5f;

    private float lastTriggerTime = -10f;
    private bool inHell = false;

    void Update()
    {
        if (player == null) return;
        float now = Time.time;

        if (!inHell && player.position.y <= hellThresholdY && now - lastTriggerTime > cooldownSeconds)
        {
            inHell = true;
            lastTriggerTime = now;
            EnterHell();
        }
        else if (inHell && allowReturn && player.position.y > hellThresholdY && now - lastTriggerTime > cooldownSeconds)
        {
            inHell = false;
            lastTriggerTime = now;
            ExitHell();
        }
    }

    private void EnterHell()
    {
        if (hellBackground) hellBackground.SetActive(true);
        if (groundBackground) groundBackground.SetActive(false);

        if (cameraFollow != null) cameraFollow.SetInHell(true);
        if (fastFallOnPit != null) fastFallOnPit.StartFastFall();
    }

    private void ExitHell()
    {
        if (hellBackground) hellBackground.SetActive(false);
        if (groundBackground) groundBackground.SetActive(true);

        if (cameraFollow != null) cameraFollow.SetInHell(false);
    }

    // ---------- método adicional que você adicionou ----------
    // Deve estar DENTRO da classe (entre as chaves acima).
    public void ForceReturnToGround()
    {
        inHell = false;
        if (hellBackground) hellBackground.SetActive(false);
        if (groundBackground) groundBackground.SetActive(true);
    }
}
