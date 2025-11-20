using UnityEngine;

public class MoveLeft : MonoBehaviour
{
    [Header("Fallback / Tuning")]
    [Tooltip("Velocidade local usada quando não há GameManager (unidades/s).")]
    public float baseSpeed = 5f;

    [Tooltip("Multiplica a velocidade do mundo para este objeto (ex: 0.5 = metade, 2 = dobro).")]
    public float speedMultiplier = 1f;

    [Tooltip("Se true, respeita o Time.timeScale (não se move quando tempo está parado).")]
    public bool respectTimeScale = true;

    void Update()
    {
        // pausa segura
        if (respectTimeScale && Time.timeScale <= 0f) return;

        // pega a velocidade do mundo (se disponível) ou usa a base local
        float worldSpeed = baseSpeed;
        if (GameManager.Instance != null)
        {
            worldSpeed = GameManager.Instance.GetCurrentSpeed();
            // caso GameManager retorne 0 (por algum motivo), mantém fallback:
            if (Mathf.Approximately(worldSpeed, 0f))
                worldSpeed = baseSpeed;
        }

        // aplica multiplicador local e move para a esquerda
        float finalSpeed = worldSpeed * speedMultiplier;
        transform.Translate(Vector2.left * finalSpeed * Time.deltaTime);
    }
}
