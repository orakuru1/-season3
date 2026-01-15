using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera playerCamera;
    public Camera topCamera;
    public Transform TopViewPoint;
    public Transform PlayerViewPoint;

    bool isTopView = false;

    void Start()
    {
        //SetView(false); // 最初はプレイヤー視点
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

    public void SetView(bool top)
    {
        DamageTextManager.Instance.SetCamera(top ? topCamera : playerCamera);
        DamageTextManager.Instance.SetTransform(top ? TopViewPoint : PlayerViewPoint);
        playerCamera.gameObject.SetActive(!top);
        topCamera.gameObject.SetActive(top);
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
