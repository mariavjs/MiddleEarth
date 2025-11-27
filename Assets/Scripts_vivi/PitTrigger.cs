using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class PitTrigger : MonoBehaviour
{
    Transform playerTransform;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerTransform = other.transform;

        if (PitManager.Instance != null)
        {
            PitManager.Instance.StartPitForPlayer(playerTransform);
        }
    }
}