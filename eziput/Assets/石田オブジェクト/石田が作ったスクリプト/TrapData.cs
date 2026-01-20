public enum TrapType
{
    None,
    Damage,
    Stun
}

[System.Serializable]
public class TrapData
{
    public TrapType type;
    public int value;        // ダメージ量 or 行動不能ターン
    public bool oneShot = true;
}
