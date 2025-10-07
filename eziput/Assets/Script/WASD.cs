using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WASD : MonoBehaviour
{
    public float speed = 3.0f;
    public float rotationSpeed = 10f; //回転速度
    public float jumpForce = 5.0f;

    private Rigidbody rb;
    private bool isGround = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Vector3 inputDir = Vector3.zero;

        // 入力ベクトルを組み立てる
        if (Input.GetKey(KeyCode.W)) inputDir += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) inputDir += Vector3.back;
        if (Input.GetKey(KeyCode.A)) inputDir += Vector3.left;
        if (Input.GetKey(KeyCode.D)) inputDir += Vector3.right;

        // 方向が入力されていれば向きを変えて前進
        if (inputDir != Vector3.zero)
        {
            //入力方向を正規化
            inputDir.Normalize();

            // 向き変更（瞬時に回転）
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // 前進（常にforward方向に進む）
            Vector3 moveVelocity = inputDir * speed;
            rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
        }
        else
        {
            //移動していないときも水平方向の速度を止める
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        //スペースでジャンプ
        if(Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGround = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            isGround = true;
        }
    }
}
