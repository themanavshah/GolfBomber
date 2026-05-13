using UnityEngine;

public class BombBallSpawner : MonoBehaviour
{
    [SerializeField] private BombBall ballPrefab;
    [SerializeField] private float respawnDelaySeconds = 1.0f;
    [SerializeField] private float clearanceRadius = 0.15f;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private GameObject spawnIndicator;

    float _nextSpawnTime;
    bool _wasOccupied = true;

    void Start()
    {
        if (spawnOnStart)
        {
            _wasOccupied = false;
            SpawnBall();
        }
    }

    void Update()
    {
        bool occupied = IsSpawnPositionOccupied();

        if (spawnIndicator != null && spawnIndicator.activeSelf == occupied)
        {
            spawnIndicator.SetActive(!occupied);
        }

        if (occupied)
        {
            _wasOccupied = true;
            return;
        }

        if (_wasOccupied)
        {
            _nextSpawnTime = Time.time + respawnDelaySeconds;
            _wasOccupied = false;
        }

        if (Time.time >= _nextSpawnTime) SpawnBall();
    }

    bool IsSpawnPositionOccupied()
    {
        Collider[] overlaps = Physics.OverlapSphere(transform.position, clearanceRadius);
        foreach (Collider c in overlaps)
        {
            if (c.GetComponentInParent<BombBall>() != null) return true;
        }
        return false;
    }

    void SpawnBall()
    {
        if (ballPrefab == null)
        {
            Debug.LogWarning($"{nameof(BombBallSpawner)}: ballPrefab not assigned.", this);
            return;
        }
        Instantiate(ballPrefab, transform.position, ballPrefab.transform.rotation, transform);
        _nextSpawnTime = Time.time + respawnDelaySeconds;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, clearanceRadius);
    }
}
