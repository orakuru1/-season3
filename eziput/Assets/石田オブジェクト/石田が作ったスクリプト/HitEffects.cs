using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect", menuName = "HitEffects")]
public class HitEffects : ScriptableObject
{
    public int id;
    public GameObject effectPrefab;
    public float duration = 1f;
    public Vector3 offset = new Vector3(0, 0, 0);
}
