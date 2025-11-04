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
        nextTileSpawnPos = temp.GetComponent<Transform>().GetChild(1).transform.position;
    }
}
