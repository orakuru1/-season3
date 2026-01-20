using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance { get; private set; }
    public GameObject damagePrefab;
    public Transform damageTextCanvas;
    public Camera camera;
    private Transform PlayerTransform;
    public bool isView = false;

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    public void SetCamera(Camera cam, bool isTopView)
    {
        camera = cam;
        isView = isTopView;
    }

    public void SetTransform(Transform camTransform)
    {
        damageTextCanvas = camTransform;
    }

    public void ShowDamage(int damage, Transform point)
    {
        if(PlayerTransform == point && isView == false) return; // プレイヤー自身へのダメージは表示しない
        Renderer renderer = point.GetComponentInChildren<Renderer>();

        float yOffset = 0f;
        //オブジェクトの頭の上に表示するために、オブジェクトの高さを取得
        if (renderer != null) yOffset = renderer.bounds.size.y;
        //カメラの方向を取得
        Vector3 tocamera = (camera.transform.position - point.position).normalized;
        //少し前に表示するためのオフセット
        float forwardOffset = 1.5f;
        //ダメージテキストの生成位置を計算
        Vector3 spawnPos = point.position + new Vector3(0, yOffset / 2, 0f) + tocamera * forwardOffset;
        var obj = Instantiate(damagePrefab, spawnPos, damagePrefab.transform.rotation, damageTextCanvas);
        obj.GetComponentInChildren<Text>().text = damage.ToString();
        Destroy(obj, 1f); // 1秒後に削除
    }

    private void OnEnable()
    {
        GameManager.OnPlayerSpawned += SetPlayer;
    }
    private void OnDisable()
    {
        GameManager.OnPlayerSpawned -= SetPlayer;
    }
    private void SetPlayer(GameObject player)
    {
        PlayerTransform = player.transform;
    }
}
