using UnityEngine;

public class CameraFollowAdvanced : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;

    [Header("View Settings")]
    [Tooltip("カメラ距離")]
    public float distance = 3f;
    public float minDistance = 4f;
    public float maxDistance = 15f;
    public float zoomSpeed = 3f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 120f;
    private float currentYaw = 0f;

    [Header("Follow Settings")]
    public float followSmoothness = 7f;

    [Header("Collision Settings")]
    public LayerMask collisionMask;
    public float collisionBuffer = 0.3f;

    private float currentDistance;

    // ★ 常に90度固定
    private const float FIXED_ANGLE = 90f;

    public static CameraFollowAdvanced Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                SetTarget(p.transform);
            }
            else
            {
                Debug.LogWarning("CameraFollowAdvanced: Player がシーン内に見つかりませんでした。");
            }
        }
        else
        {
            SetTarget(target);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        currentDistance = distance;
        currentYaw = 0f;

        Quaternion rot = Quaternion.Euler(FIXED_ANGLE, currentYaw, 0);
        Vector3 startPos = target.position - rot * Vector3.forward * currentDistance;

        transform.position = startPos;
        //transform.rotation = rot;
    }

    void LateUpdate()
    {
        if (target == null) return;

        currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * 5f);

        // ★ 必ずここで宣言する（超重要）
        Quaternion rot = Quaternion.Euler(90f, currentYaw, 0f);
        Vector3 desiredPos = target.position - rot * Vector3.forward * currentDistance;

        // 障害物補正
        Vector3 dir = desiredPos - target.position;
        float dist = dir.magnitude;

        if (Physics.Raycast(target.position, dir.normalized, out RaycastHit hit, dist, collisionMask))
        {
            desiredPos = hit.point - dir.normalized * collisionBuffer;
        }

        // 追従
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            followSmoothness * Time.deltaTime
        );

        // ★ 角度は常に固定
        //transform.rotation = rot;
    }
}
