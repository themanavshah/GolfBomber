using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class FallIfUnsupportedOnSpawn : MonoBehaviour
{
    [SerializeField] private float checkDelay = 0.1f;
    [SerializeField] private float supportCheckDistance = 0.5f;
    [SerializeField] private float supportCheckInset = 0.05f;
    [SerializeField] private LayerMask supportMask = ~0;

    Rigidbody _rb;
    Collider _col;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
    }

    void Start()
    {
        Invoke(nameof(CheckSupport), checkDelay);
    }

    void CheckSupport()
    {
        if (_rb == null || !_rb.isKinematic) return;
        if (!SupportChecker.HasSupportBelow(_col, supportCheckDistance, supportCheckInset, supportMask))
        {
            _rb.isKinematic = false;
        }
    }
}
