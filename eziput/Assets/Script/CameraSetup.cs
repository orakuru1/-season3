using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    public Transform dungeonRoot;
    public float heightFactor = 1.3f;
    public float angle = 60f;

    void Start()
    {
        Invoke(nameof(AdjustCamera), 0.3f);
    }

    void AdjustCamera()
    {
        if (dungeonRoot == null)
        {
            Debug.LogWarning("CameraSetup: dungeonRoot が設定されていません");
            return;
        }

        Renderer[] renderers = dungeonRoot.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = new Bounds(renderers[0].bounds.center, Vector3.zero);
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);

        Vector3 center = bounds.center;
        float size = Mathf.Max(bounds.size.x, bounds.size.z);
        float height = size * heightFactor;

        transform.position = center + new Vector3(0, height, -height * 0.6f);
        transform.LookAt(center);
        transform.rotation = Quaternion.Euler(angle, 0, 0);
    }
}
