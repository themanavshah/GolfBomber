using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ClubHead : MonoBehaviour
{
    [SerializeField] private float hitForceMultiplier = 1.5f;
    [SerializeField] private float minHitSpeed = 0.5f;
    [SerializeField] private float maxBallSpeed = 30f;

    Vector3 _previousPosition;
    Vector3 _velocity;

    void OnEnable()
    {
        _previousPosition = transform.position;
        _velocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        Vector3 currentPosition = transform.position;
        _velocity = (currentPosition - _previousPosition) / Time.fixedDeltaTime;
        _previousPosition = currentPosition;
    }

    void OnCollisionEnter(Collision collision)
    {
        TryHitBall(collision.collider);
    }

    void OnTriggerEnter(Collider other)
    {
        TryHitBall(other);
    }

    void TryHitBall(Collider other)
    {
        BombBall ball = other.GetComponentInParent<BombBall>();
        if (ball == null || ball.HasBeenHit) return;

        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        if (ballRb == null) return;

        float speed = _velocity.magnitude;
        if (speed < minHitSpeed) return;

        Vector3 hitVelocity = _velocity * hitForceMultiplier;
        if (hitVelocity.magnitude > maxBallSpeed)
        {
            hitVelocity = hitVelocity.normalized * maxBallSpeed;
        }

        ball.MarkHit();
        ballRb.linearVelocity = hitVelocity;
    }
}
