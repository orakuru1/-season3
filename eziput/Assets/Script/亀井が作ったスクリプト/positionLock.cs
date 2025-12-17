using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class positionLock : MonoBehaviour
{
    private Vector3 lockedLocalPosition;

    // Start is called before the first frame update
    void Start()
    {
        //最初の位置を記録
        lockedLocalPosition = transform.position;        
    }

    void LateUpdate()
    {
        //Animatorの後で位置だけ戻す
        transform.position = lockedLocalPosition;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
