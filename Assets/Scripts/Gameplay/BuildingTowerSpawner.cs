using UnityEngine;

public class BuildingTowerSpawner : MonoBehaviour
{
    [SerializeField] private DestructibleBuilding floorPrefab;
    [SerializeField] private float floorHeight = 1f;
    [SerializeField] private float spawnIntervalSeconds = 3f;
    [SerializeField] private int maxFloors = 32;
    [SerializeField] private bool spawnFirstImmediately = true;

    int _nextIndex;
    float _nextSpawnTime;

    void Start()
    {
        _nextSpawnTime = spawnFirstImmediately ? Time.time : Time.time + spawnIntervalSeconds;
    }

    void Update()
    {
        if (_nextIndex >= maxFloors) return;
        if (Time.time < _nextSpawnTime) return;

        SpawnFloor(_nextIndex);
        _nextIndex++;
        _nextSpawnTime = Time.time + spawnIntervalSeconds;
    }

    void SpawnFloor(int index)
    {
        if (floorPrefab == null)
        {
            Debug.LogWarning($"{nameof(BuildingTowerSpawner)}: floorPrefab not assigned.", this);
            return;
        }

        Vector3 pos = transform.position + Vector3.up * floorHeight * index;
        Instantiate(floorPrefab, pos, floorPrefab.transform.rotation, transform);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 size = new Vector3(0.6f, floorHeight, 0.6f);
        for (int i = 0; i < maxFloors; i++)
        {
            Vector3 center = transform.position + Vector3.up * (floorHeight * i + floorHeight * 0.5f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
