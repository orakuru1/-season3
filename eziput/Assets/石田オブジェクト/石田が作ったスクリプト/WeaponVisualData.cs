using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Visual")]
public class WeaponVisualData : ScriptableObject
{
    public GameObject prefab;
    public Vector3 localPosition;
    public Vector3 localRotation;
    public AudioClip swingSound;
    public GameObject TrailEffect;
}
