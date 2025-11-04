using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    public GameObject groundPrefab;
    private Vector3 nextTileSpawnPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nextTileSpawnPos = new Vector3(-8, -3, 0f);
        SpawnTile();
    }

    // Update is called once per frame
    public void SpawnTile()
    {
        GameObject temp = Instantiate(groundPrefab, nextTileSpawnPos, Quaternion.identity);
        Transform endpoint = temp.transform.Find("Endpoint");
        if (endpoint != null)
        {
            nextTileSpawnPos = endpoint.position;
        }
        else
        {
            Debug.LogError("Ground prefab não possui filho 'Endpoint'.");
        }

    }
}
