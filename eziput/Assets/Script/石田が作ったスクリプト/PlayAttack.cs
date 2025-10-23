using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAttack : MonoBehaviour
{
    [SerializeField] private Animator animator;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayAttackAnimation()
    {
        animator.SetTrigger("Hit");
    }
    
    public void PlayEndAnimation()
    {
        animator.SetTrigger("End");
    }
}
