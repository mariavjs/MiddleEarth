using UnityEngine;

[AddComponentMenu("Game/PitYTrigger")]
public class PitYTrigger : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public CameraFollow cameraFollow;      // arraste aqui a Main Camera (que tem CameraFollow)
    public FastFallOnPit fastFallOnPit;    // arraste a instância do FastFallOnPit
    public GameObject hellBackground;      // opcional
    public GameObject groundBackground;    // opcional

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
        // 1) ativa inferno imediatamente (para evitar gap visual)
        if (hellBackground) hellBackground.SetActive(true);
        if (groundBackground) groundBackground.SetActive(false);

        // 2) avisa a câmera (faz transição Y)
        if (cameraFollow != null) cameraFollow.SetInHell(true);

        // 3) inicia queda rápida no player
        if (fastFallOnPit != null) fastFallOnPit.StartFastFall();
    }

    private void ExitHell()
    {
        if (hellBackground) hellBackground.SetActive(false);
        if (groundBackground) groundBackground.SetActive(true);

        if (cameraFollow != null) cameraFollow.SetInHell(false);
    }
}
