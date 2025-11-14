using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveanimation : MonoBehaviour
{
    //public Animator animator;

    private Vector3 move;
    // Start is called before the first frame update
    void Start()
    {
        //animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W)) move = Vector3.forward;
        else if (Input.GetKey(KeyCode.S)) move = Vector3.back;
        else if (Input.GetKey(KeyCode.A)) move = Vector3.left;
        else if (Input.GetKey(KeyCode.D)) move = Vector3.right;
        else move = Vector3.zero;

        if (move != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), 0.2f);
            //transform.Translate(move * Time.deltaTime * 2, Space.World);
            //animator.SetInteger("３種の行動", 1); // 歩く
        }
        else
        {
            //animator.SetInteger("３種の行動", 0); // 待機
        }


    }
}
