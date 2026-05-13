using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DestructibleBuilding : MonoBehaviour
{
    [SerializeField] private BuildingType type;
    [SerializeField] private bool destroyBallOnHit = true;
    [SerializeField] private float supportCheckDistance = 0.5f;
    [SerializeField] private float supportCheckInset = 0.05f;
    [SerializeField] private LayerMask supportMask = ~0;

    static event Action OnAnyBuildingDestroyed;

    bool _destroyed;
    Rigidbody _rb;
    Collider _col;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
        OnAnyBuildingDestroyed += HandlePeerDestroyed;
    }

    void OnDestroy()
    {
        OnAnyBuildingDestroyed -= HandlePeerDestroyed;
    }

    void HandlePeerDestroyed()
    {
        if (_destroyed) return;
        if (_rb == null || !_rb.isKinematic) return;
        Invoke(nameof(DoSupportCheck), 0.05f);
    }

    void DoSupportCheck()
    {
        if (_destroyed) return;
        if (_rb == null || !_rb.isKinematic) return;
        if (!HasSupportBelow()) _rb.isKinematic = false;
    }

    bool HasSupportBelow()
    {
        return SupportChecker.HasSupportBelow(_col, supportCheckDistance, supportCheckInset, supportMask);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Building] OnCollisionEnter on {name} with {collision.collider.name}");
        TryDestroy(collision.collider);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Building] OnTriggerEnter on {name} with {other.name}");
        TryDestroy(other);
    }

    void TryDestroy(Collider hitter)
    {
        if (_destroyed) return;

        BombBall ball = hitter.GetComponentInParent<BombBall>();
        if (ball == null)
        {
            Debug.Log($"[Building] Hitter {hitter.name} is not a BombBall — ignored.");
            return;
        }
        if (!ball.HasBeenHit)
        {
            Debug.Log($"[Building] BombBall {ball.name} hasn't been hit yet — ignored.");
            return;
        }

        _destroyed = true;

        if (type == null)
        {
            Debug.LogWarning($"[Building] {name} has no BuildingType assigned — destruction ignored.", this);
            return;
        }

        Debug.Log($"[Score] +{type.points} — destroyed {name} ({type.displayName})");

        if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(type.points);
        if (DestructionTracker.Instance != null) DestructionTracker.Instance.RegisterDestruction(type.displayName, type.points);

        if (type.destroySound != null)
        {
            GameObject oneShot = new GameObject("OneShotAudio");
            oneShot.transform.position = transform.position;
            AudioSource src = oneShot.AddComponent<AudioSource>();
            src.clip = type.destroySound;
            src.volume = type.destroyVolume;
            src.spatialBlend = type.destroySoundSpatial;
            src.Play();
            Destroy(oneShot, type.destroySound.length + 0.1f);
        }

        if (_col != null) _col.enabled = false;

        OnAnyBuildingDestroyed?.Invoke();

        if (destroyBallOnHit) Destroy(ball.gameObject);
        Destroy(gameObject);
    }
}
