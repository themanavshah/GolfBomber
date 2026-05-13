using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BombBall : MonoBehaviour
{
    [SerializeField] private float despawnAfterSeconds = 20f;
    [SerializeField] private float despawnIfBelowY = -50f;

    public bool HasBeenHit { get; private set; }

    Rigidbody _rb;
    float _spawnTime;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _spawnTime = Time.time;
        _rb.isKinematic = true;
    }

    public void MarkHit()
    {
        if (HasBeenHit) return;
        HasBeenHit = true;
        transform.SetParent(null, true);
        _rb.isKinematic = false;
        Debug.Log($"[Ball] {name} MarkHit — now dynamic");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (HasBeenHit)
            Debug.Log($"[Ball] {name} collided with {collision.collider.name}");
    }

    void Update()
    {
        if (Time.time - _spawnTime > despawnAfterSeconds || transform.position.y < despawnIfBelowY)
        {
            Destroy(gameObject);
        }
    }
}
