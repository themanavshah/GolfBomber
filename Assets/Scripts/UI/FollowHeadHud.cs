using UnityEngine;

public class FollowHeadHud : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private Vector3 localOffset = new Vector3(0.4f, 0.3f, 1.0f);
    [SerializeField] private bool faceCamera = true;

    void Start()
    {
        if (head == null && Camera.main != null) head = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (head == null) return;

        Vector3 worldPos = head.position
                         + head.right * localOffset.x
                         + head.up * localOffset.y
                         + head.forward * localOffset.z;

        transform.position = worldPos;
        if (faceCamera) transform.rotation = head.rotation;
    }
}
