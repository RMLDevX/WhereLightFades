using UnityEngine;

public class Scene2Camera : MonoBehaviour
{
    public float smoothness = 0.1f;
    public Vector3 offset = new Vector3(0, 1, -10);

    [Header("Camera Boundaries")]
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -5f;
    public float maxY = 5f;

    [Header("Gizmos Settings")]
    public bool showGizmos = true;
    public Color minBoundaryColor = Color.yellow;
    public Color maxBoundaryColor = Color.red;
    public Color cameraBoundsColor = Color.cyan;

    private Transform target;
    private Vector3 velocity = Vector3.zero;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        FindPlayer();
    }

    void Update()
    {
        if (target == null)
        {
            FindPlayer();
            return;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        // Apply boundaries to the target position
        targetPosition = GetBoundedPosition(targetPosition);

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothness);
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            Debug.Log("Player found for camera!");
        }
    }

    Vector3 GetBoundedPosition(Vector3 targetPosition)
    {
        if (cam == null) return targetPosition;

        // Calculate camera dimensions
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        // Calculate boundaries (considering camera size)
        float minXBound = minX + width / 2f;
        float maxXBound = maxX - width / 2f;
        float minYBound = minY + height / 2f;
        float maxYBound = maxY - height / 2f;

        // Clamp the position
        targetPosition.x = Mathf.Clamp(targetPosition.x, minXBound, maxXBound);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minYBound, maxYBound);

        return targetPosition;
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // Draw level boundaries with different colors for min and max
        Vector3 bottomLeft = new Vector3(minX, minY, 0);
        Vector3 bottomRight = new Vector3(maxX, minY, 0);
        Vector3 topLeft = new Vector3(minX, maxY, 0);
        Vector3 topRight = new Vector3(maxX, maxY, 0);

        // Draw min boundaries in yellow
        Gizmos.color = minBoundaryColor;
        Gizmos.DrawLine(bottomLeft, topLeft); // Left edge (minX)
        Gizmos.DrawLine(bottomLeft, bottomRight); // Bottom edge (minY)

        // Draw max boundaries in red
        Gizmos.color = maxBoundaryColor;
        Gizmos.DrawLine(bottomRight, topRight); // Right edge (maxX)
        Gizmos.DrawLine(topLeft, topRight); // Top edge (maxY)

        // Draw camera boundaries if we have a camera reference
        Camera currentCam = cam != null ? cam : GetComponent<Camera>();
        if (currentCam != null && Application.isPlaying)
        {
            Gizmos.color = cameraBoundsColor;

            // Calculate camera bounds at current position
            float height = 2f * currentCam.orthographicSize;
            float width = height * currentCam.aspect;

            Vector3 camBottomLeft = new Vector3(
                transform.position.x - width / 2f,
                transform.position.y - height / 2f,
                0
            );

            Vector3 camBottomRight = new Vector3(
                transform.position.x + width / 2f,
                transform.position.y - height / 2f,
                0
            );

            Vector3 camTopLeft = new Vector3(
                transform.position.x - width / 2f,
                transform.position.y + height / 2f,
                0
            );

            Vector3 camTopRight = new Vector3(
                transform.position.x + width / 2f,
                transform.position.y + height / 2f,
                0
            );

            // Draw camera bounds
            Gizmos.DrawLine(camBottomLeft, camBottomRight);
            Gizmos.DrawLine(camBottomRight, camTopRight);
            Gizmos.DrawLine(camTopRight, camTopLeft);
            Gizmos.DrawLine(camTopLeft, camBottomLeft);
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || !Application.isPlaying) return;

        // Draw the effective camera boundaries (where camera can actually move)
        Camera currentCam = cam != null ? cam : GetComponent<Camera>();
        if (currentCam != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Green with transparency for effective boundaries

            float height = 2f * currentCam.orthographicSize;
            float width = height * currentCam.aspect;

            // Calculate effective boundaries
            float effectiveMinX = minX + width / 2f;
            float effectiveMaxX = maxX - width / 2f;
            float effectiveMinY = minY + height / 2f;
            float effectiveMaxY = maxY - height / 2f;

            // Draw effective boundary area
            Vector3 effectiveBottomLeft = new Vector3(effectiveMinX, effectiveMinY, 0);
            Vector3 effectiveBottomRight = new Vector3(effectiveMaxX, effectiveMinY, 0);
            Vector3 effectiveTopLeft = new Vector3(effectiveMinX, effectiveMaxY, 0);
            Vector3 effectiveTopRight = new Vector3(effectiveMaxX, effectiveMaxY, 0);

            // Draw filled rectangle for effective boundaries
            DrawSolidRectangle(effectiveBottomLeft, effectiveTopRight, new Color(0f, 1f, 0f, 0.1f));

            // Draw outline in green
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            Gizmos.DrawLine(effectiveBottomLeft, effectiveBottomRight);
            Gizmos.DrawLine(effectiveBottomRight, effectiveTopRight);
            Gizmos.DrawLine(effectiveTopRight, effectiveTopLeft);
            Gizmos.DrawLine(effectiveTopLeft, effectiveBottomLeft);
        }
    }

    // Helper method to draw a solid rectangle
    void DrawSolidRectangle(Vector3 bottomLeft, Vector3 topRight, Color color)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        vertices[0] = bottomLeft;
        vertices[1] = new Vector3(topRight.x, bottomLeft.y, bottomLeft.z);
        vertices[2] = topRight;
        vertices[3] = new Vector3(bottomLeft.x, topRight.y, bottomLeft.z);

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(0, 1);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        Gizmos.color = color;
        Gizmos.DrawMesh(mesh);
    }
}