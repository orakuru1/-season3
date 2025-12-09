using UnityEngine;

public class CameraFollowAdvanced : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;

    [Header("View Settings")]
    public float angle = 45f;
    public float distance = 8f;
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
    public static CameraFollowAdvanced Instance;
    void Awake()
    {
        Instance = this;
    }

    //----------------------------------------
    // ★ Start時にプレイヤーを探してセット
    //----------------------------------------
    void Start()
    {
        // target が未設定ならシーン内の Player を探す
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
            // Inspectorで設定済みなら即セット
            SetTarget(target);
        }
    }

    //----------------------------------------
    // ★ ターゲット設定用メソッド（重要）
    //----------------------------------------
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        // 初期距離セット
        currentDistance = distance;
        currentYaw = 0f;

        // 初期位置計算して“瞬間的に”移動
        Quaternion rot = Quaternion.Euler(angle, currentYaw, 0);
        Vector3 startPos = target.position - rot * Vector3.forward * currentDistance;

        transform.position = startPos;
        transform.LookAt(target.position);
    }

    //----------------------------------------
    // ★ 毎フレームの追従処理
    //----------------------------------------
    void LateUpdate()
    {
        if (target == null) return;

        // 右ドラッグで回転
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            currentYaw += mouseX * rotationSpeed * Time.deltaTime;
        }

        // マウスホイールでズーム
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * 5f);

        // 理想位置
        Quaternion rot = Quaternion.Euler(angle, currentYaw, 0);
        Vector3 desiredPos = target.position - rot * Vector3.forward * currentDistance;

        // 障害物補正
        Vector3 dir = desiredPos - target.position;
        float dist = dir.magnitude;

        if (Physics.Raycast(target.position, dir.normalized, out RaycastHit hit, dist, collisionMask))
        {
            desiredPos = hit.point - dir.normalized * collisionBuffer;
        }

        // スムーズ追従
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmoothness * Time.deltaTime);

        // 注視
        transform.LookAt(target.position);
    }
}
