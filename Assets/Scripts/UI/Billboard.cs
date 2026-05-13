using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool keepUpright = true;

    void Start()
    {
        if (target == null && Camera.main != null) target = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 lookDir = transform.position - target.position;
        if (keepUpright) lookDir.y = 0f;
        if (lookDir.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
