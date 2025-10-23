using UnityEngine;
using System.Collections;

public class CameraSetup : MonoBehaviour
{
    public ElementGenerator generator;

    [Header("カメラ配置設定")]
    public float startHeightOffset = 40f; // ズームアウト時の高さ
    public float endHeightOffset = 20f;   // 最終的な高さ
    public float tiltAngle = 60f;         // 俯瞰角度
    public float margin = 5f;             // 見切れ防止余白

    [Header("ズームアニメーション設定")]
    public float zoomDuration = 3f;       // ズームにかかる秒数
    public bool autoPlay = true;          // 起動時に自動ズーム

    private Vector3 targetCenter;
    private float cellSize;
    private int mapWidth, mapHeight;

    void Start()
    {
        if (generator == null)
        {
            generator = FindObjectOfType<ElementGenerator>();
            if (generator == null)
            {
                Debug.LogError("❌ ElementGenerator が見つかりません。");
                return;
            }
        }

        // マップ情報を取得
        mapWidth = generator.GetMapWidth();
        mapHeight = generator.GetMapHeight();
        cellSize = generator.cellSize;

        // マップの中心を算出
        float centerX = (mapWidth * cellSize) / 2f - cellSize / 2f;
        float centerZ = (mapHeight * cellSize) / 2f - cellSize / 2f;
        targetCenter = new Vector3(centerX, 0, centerZ);

        if (autoPlay)
            StartCoroutine(CameraZoomAnimation());
        else
            SetCameraPosition(endHeightOffset); // アニメなし配置
    }

    IEnumerator CameraZoomAnimation()
    {
        float elapsed = 0f;

        // 開始位置と終了位置
        Vector3 startPos = CalculateCameraPosition(startHeightOffset);
        Vector3 endPos = CalculateCameraPosition(endHeightOffset);

        while (elapsed < zoomDuration)
        {
            float t = elapsed / zoomDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.LookAt(targetCenter);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 最終位置へスナップ
        transform.position = endPos;
        transform.LookAt(targetCenter);
    }

    Vector3 CalculateCameraPosition(float height)
    {
        // カメラがマップ全体を見下ろすようにZ方向を調整
        float distance = (mapWidth + mapHeight) / 2f + margin;
        return new Vector3(targetCenter.x, height, targetCenter.z - distance);
    }

    void SetCameraPosition(float height)
    {
        transform.position = CalculateCameraPosition(height);
        transform.LookAt(targetCenter);
    }
}
