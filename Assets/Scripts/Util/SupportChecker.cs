using UnityEngine;

public static class SupportChecker
{
    public static bool HasSupportBelow(Collider col, float distance, float inset, LayerMask mask)
    {
        if (col == null) return false;

        Bounds b = col.bounds;
        float y = b.min.y - 0.005f;
        float minX = b.min.x + inset, maxX = b.max.x - inset;
        float minZ = b.min.z + inset, maxZ = b.max.z - inset;
        float cx = b.center.x, cz = b.center.z;

        Vector3[] origins =
        {
            new Vector3(cx, y, cz),
            new Vector3(minX, y, minZ),
            new Vector3(minX, y, maxZ),
            new Vector3(maxX, y, minZ),
            new Vector3(maxX, y, maxZ),
        };

        foreach (Vector3 o in origins)
        {
            if (Physics.Raycast(o, Vector3.down, distance, mask, QueryTriggerInteraction.Ignore))
                return true;
        }
        return false;
    }
}
