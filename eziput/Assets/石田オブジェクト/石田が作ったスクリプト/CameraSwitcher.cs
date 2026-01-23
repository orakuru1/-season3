using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera playerCamera;
    public Camera topCamera;
    public Transform TopViewPoint;
    public Transform PlayerViewPoint;

    public bool isTopView = false;
    public static CameraSwitcher Instance { get; private set; }

    void Start()
    {
        //SetView(false); // 最初はプレイヤー視点
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) // 好きなキー
        {
            isTopView = !isTopView;
            SetView(isTopView);
        }
    }

    public void SetPlayerView(GameObject playerObj)
    {
        PlayerViewPoint = playerObj.GetComponentInChildren<Canvas>().transform;
        playerCamera = playerObj.GetComponentInChildren<Camera>();
        SetView(false);
    }

    public void SetView(bool top)//Audio　Listenrはカメラにつけないほうが良いかも。BGMが最初になってしまう。
    {
        if(DamageTextManager.Instance != null)
        {
            DamageTextManager.Instance.SetCamera(top ? topCamera : playerCamera, isTopView);
            DamageTextManager.Instance.SetTransform(top ? TopViewPoint : PlayerViewPoint);
        }

        playerCamera.gameObject.SetActive(!top);
        topCamera.gameObject.SetActive(top);

        if (WallHintManager.Instance != null)
        {
            WallHintManager.Instance.SetAllVisible(!top);
            // top = 俯瞰 → 非表示
            // !top = 一人称 → 表示
        }
    }

    public void OnEnable()
    {
        GameManager.OnPlayerSpawned += SetPlayerView;
    }
    public void OnDisable()
    {
        GameManager.OnPlayerSpawned -= SetPlayerView;
    }
}
