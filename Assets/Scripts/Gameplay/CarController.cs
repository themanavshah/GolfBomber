using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class CarController : MonoBehaviour
{
    [SerializeField] private Transform driverSeat;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float turnSpeed = 60f;
    [SerializeField] private float enterDistance = 3f;
    [SerializeField] private float collisionPadding = 0.05f;
    [SerializeField] private float maxStepHeight = 0.3f;
    [SerializeField, Range(0f, 89f)] private float maxClimbableSlopeAngle = 50f;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float groundSearchAbove = 2f;
    [SerializeField] private float groundSearchBelow = 5f;
    [SerializeField] private float groundOffset = 0f;
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private float tiltAlignSpeed = 10f;
    [SerializeField] private Behaviour[] disableWhileOccupied;
    [SerializeField] private GameObject[] hideWhileOccupied;
    [SerializeField] private GameObject enterPrompt;

    Rigidbody _rb;
    bool _occupied;
    bool _wasXPressed;
    float _verticalVelocity;
    Transform _originalXrOriginParent;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
    }

    void Update()
    {
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        bool xPressed = false;
        leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out xPressed);
        if (xPressed && !_wasXPressed)
        {
            if (_occupied) Exit();
            else TryEnter();
        }
        _wasXPressed = xPressed;

        UpdateEnterPrompt();

        if (!_occupied) return;

        bool triggerPressed = false;
        leftHand.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed);

        Vector2 stick = Vector2.zero;
        leftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out stick);

        if (triggerPressed)
        {
            Vector3 moveDelta = transform.forward * forwardSpeed * Time.deltaTime;
            TryMove(moveDelta);
            transform.Rotate(Vector3.up * stick.x * turnSpeed * Time.deltaTime, Space.Self);
        }

        GroundCheckAndGravity();
    }

    void GroundCheckAndGravity()
    {
        Vector3 rayStart = transform.position + Vector3.up * groundSearchAbove;
        float rayMaxDist = groundSearchAbove + groundSearchBelow;

        RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.down, rayMaxDist, groundMask, QueryTriggerInteraction.Ignore);

        bool foundGround = false;
        float bestY = 0f;
        float bestDist = float.MaxValue;
        Vector3 bestNormal = Vector3.up;

        foreach (var h in hits)
        {
            if (h.collider.transform.IsChildOf(transform)) continue;
            if (h.distance < bestDist)
            {
                bestDist = h.distance;
                bestY = h.point.y;
                bestNormal = h.normal;
                foundGround = true;
            }
        }

        Quaternion targetRotation;

        if (foundGround)
        {
            transform.position = new Vector3(transform.position.x, bestY + groundOffset, transform.position.z);
            _verticalVelocity = 0f;

            Vector3 forwardOnSurface = Vector3.ProjectOnPlane(transform.forward, bestNormal);
            if (forwardOnSurface.sqrMagnitude < 1e-6f) forwardOnSurface = transform.forward;
            targetRotation = Quaternion.LookRotation(forwardOnSurface.normalized, bestNormal);
        }
        else
        {
            _verticalVelocity += Physics.gravity.y * gravityScale * Time.deltaTime;
            transform.position += Vector3.up * _verticalVelocity * Time.deltaTime;

            Vector3 forwardFlat = transform.forward;
            forwardFlat.y = 0f;
            if (forwardFlat.sqrMagnitude < 1e-6f) forwardFlat = Vector3.forward;
            targetRotation = Quaternion.LookRotation(forwardFlat.normalized, Vector3.up);
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tiltAlignSpeed * Time.deltaTime);
    }

    void LateUpdate()
    {
        if (!_occupied) return;
        if (xrOrigin == null || driverSeat == null) return;

        xrOrigin.SetPositionAndRotation(driverSeat.position, driverSeat.rotation);
    }

    void TryMove(Vector3 delta)
    {
        if (delta.sqrMagnitude < 1e-8f) return;

        float distance = delta.magnitude;
        Vector3 direction = delta / distance;

        if (!_rb.SweepTest(direction, out RaycastHit hit, distance + collisionPadding, QueryTriggerInteraction.Ignore))
        {
            transform.position += delta;
            return;
        }

        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
        if (slopeAngle <= maxClimbableSlopeAngle)
        {
            Vector3 slopeDir = Vector3.ProjectOnPlane(direction, hit.normal);
            if (slopeDir.sqrMagnitude > 1e-6f)
            {
                slopeDir.Normalize();
                Vector3 slideDelta = slopeDir * distance;

                if (!_rb.SweepTest(slopeDir, out RaycastHit _, distance + collisionPadding, QueryTriggerInteraction.Ignore))
                {
                    transform.position += slideDelta;
                    return;
                }
            }
        }

        Vector3 originalPos = transform.position;
        transform.position += Vector3.up * maxStepHeight;

        if (!_rb.SweepTest(direction, out RaycastHit _, distance + collisionPadding, QueryTriggerInteraction.Ignore))
        {
            transform.position += delta;
            return;
        }

        transform.position = originalPos;
        float safeDistance = Mathf.Max(0f, hit.distance - collisionPadding);
        transform.position += direction * safeDistance;
    }

    void TryEnter()
    {
        if (xrOrigin == null || driverSeat == null)
        {
            Debug.LogWarning($"{nameof(CarController)}: xrOrigin or driverSeat not assigned.", this);
            return;
        }

        float dist = Vector3.Distance(xrOrigin.position, transform.position);
        if (dist > enterDistance)
        {
            Debug.Log($"[Car] Too far to enter ({dist:F1}m). Approach the car.");
            return;
        }

        _originalXrOriginParent = xrOrigin.parent;
        xrOrigin.SetPositionAndRotation(driverSeat.position, driverSeat.rotation);
        xrOrigin.SetParent(transform, true);
        SetLocomotionEnabled(false);
        _occupied = true;
        Debug.Log("[Car] Entered.");
    }

    void Exit()
    {
        if (xrOrigin == null || exitPoint == null)
        {
            Debug.LogWarning($"{nameof(CarController)}: xrOrigin or exitPoint not assigned.", this);
            return;
        }

        xrOrigin.SetParent(_originalXrOriginParent, true);
        xrOrigin.SetPositionAndRotation(exitPoint.position, exitPoint.rotation);
        SetLocomotionEnabled(true);
        _occupied = false;
        Debug.Log("[Car] Exited.");
    }

    void UpdateEnterPrompt()
    {
        if (enterPrompt == null) return;

        bool inRange = !_occupied
                       && xrOrigin != null
                       && Vector3.Distance(xrOrigin.position, transform.position) <= enterDistance;

        if (enterPrompt.activeSelf != inRange) enterPrompt.SetActive(inRange);
    }

    void SetLocomotionEnabled(bool enabled)
    {
        if (disableWhileOccupied != null)
        {
            foreach (Behaviour b in disableWhileOccupied)
            {
                if (b != null) b.enabled = enabled;
            }
        }
        if (hideWhileOccupied != null)
        {
            foreach (GameObject go in hideWhileOccupied)
            {
                if (go != null) go.SetActive(enabled);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (driverSeat != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(driverSeat.position, 0.2f);
            Gizmos.DrawLine(driverSeat.position, driverSeat.position + driverSeat.forward * 0.5f);
        }
        if (exitPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(exitPoint.position, 0.2f);
            Gizmos.DrawLine(exitPoint.position, exitPoint.position + exitPoint.forward * 0.5f);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enterDistance);
    }
}
