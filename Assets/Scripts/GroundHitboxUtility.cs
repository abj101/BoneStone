using UnityEngine;

public static class GroundHitboxUtility
{
    public static Collider[] OverlapBoxOnGround(
        Transform origin,
        Vector3 localOffset,
        Vector2 sizeXZ,
        float height,
        LayerMask mask,
        QueryTriggerInteraction queryTriggers = QueryTriggerInteraction.Collide)
    {
        if (origin == null) return System.Array.Empty<Collider>();

        Vector3 center = origin.TransformPoint(localOffset);
        Quaternion rotation = Quaternion.Euler(0f, origin.eulerAngles.y, 0f);

        Vector3 halfExtents = new Vector3(
            Mathf.Max(0f, sizeXZ.x) * 0.5f,
            Mathf.Max(0.01f, height) * 0.5f,
            Mathf.Max(0f, sizeXZ.y) * 0.5f
        );

        return Physics.OverlapBox(center, halfExtents, rotation, mask, queryTriggers);
    }

    public static bool RaycastOnGround(
        Vector3 origin,
        Vector3 forward,
        float range,
        LayerMask mask,
        out RaycastHit hit,
        QueryTriggerInteraction queryTriggers = QueryTriggerInteraction.Ignore)
    {
        Vector3 dir = new Vector3(forward.x, 0f, forward.z);
        if (dir.sqrMagnitude < 0.0001f)
        {
            hit = default;
            return false;
        }

        dir.Normalize();
        return Physics.Raycast(origin, dir, out hit, range, mask, queryTriggers);
    }

    public static void DrawGizmoBoxOnGround(
        Transform origin,
        Vector3 localOffset,
        Vector2 sizeXZ,
        float height)
    {
        if (origin == null) return;

        Vector3 center = origin.TransformPoint(localOffset);
        Quaternion rotation = Quaternion.Euler(0f, origin.eulerAngles.y, 0f);
        Vector3 size = new Vector3(sizeXZ.x, Mathf.Max(0.01f, height), sizeXZ.y);

        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size);
        Gizmos.matrix = old;
    }
}

