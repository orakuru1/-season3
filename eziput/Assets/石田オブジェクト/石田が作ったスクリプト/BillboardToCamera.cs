using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // カメラの方向を向く
        transform.rotation = Quaternion.LookRotation(
            transform.position - cam.transform.position
        );
    }
}
