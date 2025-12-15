using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance { get; private set; }
    public GameObject damagePrefab;
    public Transform damageTextCanvas;

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    public void ShowDamage(int damage, Transform point)
    {
        Renderer renderer = point.GetComponentInChildren<Renderer>();

        float yOffset = 0f;
        if (renderer != null)
        {
            yOffset = renderer.bounds.size.y;        
        }

        Vector3 spawnPos = point.position + new Vector3(0, yOffset + 0.2f, 0);
        var obj = Instantiate(damagePrefab, spawnPos, damagePrefab.transform.rotation, damageTextCanvas);
        obj.GetComponentInChildren<Text>().text = damage.ToString();
        Destroy(obj, 0.7f); // 1秒後に削除
    }
}
