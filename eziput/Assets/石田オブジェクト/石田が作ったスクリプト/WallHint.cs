using UnityEngine;

public class WallHint : MonoBehaviour
{
    Renderer[] renderers;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        SetVisible(false); // 初期は非表示
    }

    public void SetVisible(bool visible)
    {
        foreach (var r in renderers)
            r.enabled = visible;
    }
}
