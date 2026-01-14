using UnityEngine;

public class MagicFireOrbit : MonoBehaviour
{
    public Transform[] fires;
    public float radius = 3f;
    public float rotateSpeed = 60f; // 度/秒

    void Start()
    {
        for (int i = 0; i < fires.Length; i++)
        {
            float angle = i * (360f / fires.Length);
            Vector3 pos = GetCirclePosition(angle);
            fires[i].localPosition = pos;
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    Vector3 GetCirclePosition(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(
            Mathf.Cos(rad) * radius,
            0.5f, // 少し浮かせる
            Mathf.Sin(rad) * radius
        );
    }
}
