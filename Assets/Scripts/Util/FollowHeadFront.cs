using UnityEngine;

public class FollowHeadFront : MonoBehaviour
{
    [SerializeField] private Transform headTarget;
    [SerializeField] private float forwardDistance = 0.5f;
    [SerializeField] private float verticalOffset = -0.3f;
    [SerializeField] private bool matchHeadYaw = true;

    void LateUpdate()
    {
        if (headTarget == null) return;

        Vector3 forward = headTarget.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f) return;
        forward.Normalize();

        transform.position = headTarget.position + forward * forwardDistance + Vector3.up * verticalOffset;

        if (matchHeadYaw)
        {
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }
}
