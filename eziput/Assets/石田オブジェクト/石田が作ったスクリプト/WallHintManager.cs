using System.Collections.Generic;
using UnityEngine;

public class WallHintManager : MonoBehaviour
{
    public static WallHintManager Instance { get; private set; }

    public List<WallHint> hints = new List<WallHint>();

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    public void Register(WallHint hint)
    {
        if (!hints.Contains(hint))
            hints.Add(hint);
    }

    public void SetAllVisible(bool visible)
    {
        foreach (var h in hints)
            h.SetVisible(visible);
    }

    public void Reset()
    {
        hints.Clear();
    }

    public void RemoveHint(WallHint wl)
    {
        hints.Remove(wl);
    }
}
