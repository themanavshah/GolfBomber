using UnityEngine;

public class FollowRigSide : MonoBehaviour
{
    [SerializeField] private Transform rig;
    [SerializeField] private Transform head;
    [SerializeField] private Vector3 localOffset = new Vector3(1.0f, 1.5f, 0f);
    [SerializeField] private bool faceCamera = true;

    void Start()
    {
        if (head == null && Camera.main != null) head = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (rig == null) return;

        Vector3 worldPos = rig.position
                         + rig.right * localOffset.x
                         + Vector3.up * localOffset.y
                         + rig.forward * localOffset.z;

        transform.position = worldPos;

        if (faceCamera && head != null)
        {
            Vector3 lookDir = transform.position - head.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }
    }
}
