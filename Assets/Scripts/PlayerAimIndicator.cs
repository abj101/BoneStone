using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAimIndicator : MonoBehaviour
{
    [Header("Circle")]
    [SerializeField] private float circleRadius = 0.65f;
    [SerializeField] private int circleSegments = 48;
    [SerializeField] private float lineWidth = 0.04f;
    [SerializeField] private Color circleColor = new Color(1f, 1f, 1f, 0.85f);

    [Header("Triangle Pointer")]
    [SerializeField] private float triSize = 0.18f;
    [SerializeField] private Color triColor = new Color(1f, 1f, 1f, 0.95f);

    [Header("Ground")]
    [SerializeField] private float groundOffset = 0.06f;

    private PlayerController _pc;
    private LineRenderer _circleLR;
    private MeshFilter _triMeshFilter;
    private MeshRenderer _triRenderer;
    private Material _sharedMat;

    private void Awake()
    {
        _pc = GetComponent<PlayerController>();
        BuildIndicators();
    }

    private void BuildIndicators()
    {
        _sharedMat = new Material(Shader.Find("Sprites/Default"));
        _sharedMat.renderQueue = 2500;

        // Circle
        var circleObj = new GameObject("AimCircle");
        circleObj.transform.SetParent(null);
        _circleLR = circleObj.AddComponent<LineRenderer>();
        _circleLR.loop = true;
        _circleLR.positionCount = circleSegments;
        _circleLR.startWidth = lineWidth;
        _circleLR.endWidth = lineWidth;
        _circleLR.useWorldSpace = true;
        _circleLR.numCapVertices = 2;
        _circleLR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _circleLR.receiveShadows = false;

        Material circleMat = new Material(_sharedMat);
        circleMat.color = circleColor;
        _circleLR.material = circleMat;

        // Triangle pointer
        var triObj = new GameObject("AimTriangle");
        triObj.transform.SetParent(null);
        _triMeshFilter = triObj.AddComponent<MeshFilter>();
        _triRenderer = triObj.AddComponent<MeshRenderer>();
        _triRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _triRenderer.receiveShadows = false;

        Material triMat = new Material(_sharedMat);
        triMat.color = triColor;
        _triRenderer.material = triMat;
    }

    private void LateUpdate()
    {
        float groundY = GetGroundY();
        UpdateCircle(groundY);
        UpdateTriangle(groundY);
    }

    private float GetGroundY()
    {
        Vector3 pos = transform.position;
        int ownerLayer = gameObject.layer;
        int groundMask = ~(1 << ownerLayer);

        if (Physics.Raycast(new Vector3(pos.x, pos.y + 0.5f, pos.z), Vector3.down, out RaycastHit hit, 10f, groundMask, QueryTriggerInteraction.Ignore))
            return hit.point.y + groundOffset;

        return pos.y - 1f + groundOffset;
    }

    private void UpdateCircle(float groundY)
    {
        Vector3 origin = transform.position;
        for (int i = 0; i < circleSegments; i++)
        {
            float angle = (float)i / circleSegments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * circleRadius;
            float z = Mathf.Sin(angle) * circleRadius;
            _circleLR.SetPosition(i, new Vector3(origin.x + x, groundY, origin.z + z));
        }
    }

    private void UpdateTriangle(float groundY)
    {
        Vector3 origin = transform.position;
        Vector3 facing = _pc.FacingDir;
        if (facing.sqrMagnitude < 0.001f) facing = transform.forward;
        facing.y = 0f;
        facing.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, facing);
        Vector3 center = new Vector3(origin.x, groundY, origin.z);

        // Base of triangle on the circle; tip outside the circle
        Vector3 tip   = center + facing * (circleRadius + triSize);
        Vector3 baseL = center + facing * circleRadius + right * (triSize * 0.5f);
        Vector3 baseR = center + facing * circleRadius - right * (triSize * 0.5f);

        var mesh = new Mesh();
        mesh.vertices = new Vector3[] { tip, baseL, baseR };
        mesh.triangles = new int[] { 0, 1, 2 };
        mesh.RecalculateNormals();

        _triMeshFilter.mesh = mesh;
        _triRenderer.transform.position = Vector3.zero;
        _triRenderer.transform.rotation = Quaternion.identity;
        _triRenderer.transform.localScale = Vector3.one;
    }

    private void OnDestroy()
    {
        if (_circleLR != null) Destroy(_circleLR.gameObject);
        if (_triMeshFilter != null) Destroy(_triMeshFilter.gameObject);
        if (_sharedMat != null) Destroy(_sharedMat);
    }
}
