using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateAnimation : StateMachineBehaviour
{

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
        {
            string stateName = clipInfo[0].clip.name;
            Debug.Log($"現在のアニメーション名: {stateName}");
        }
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
