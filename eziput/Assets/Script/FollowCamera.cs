using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target; //プレイヤー
    public Vector3 offset = new Vector3(0, 10, -5); //カメラの相対位置
    public float smoothTime = 0.2f;  //滑らかさ(小さいほど速い)
    private Vector3 velocity = Vector3.zero; //SmoothDamp用の内部変数
    void LateUpdate()
    {
        if(target == null) return;

        Vector3 targetPosition = target.position + offset;  //目標位置
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        transform.LookAt(target); //カメラが常にプレイヤーを見る
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
