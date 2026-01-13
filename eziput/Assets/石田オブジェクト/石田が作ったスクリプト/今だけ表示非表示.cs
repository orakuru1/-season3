using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 今だけ表示非表示 : MonoBehaviour
{
    public GameObject blackhole;
    public GameObject hasira;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            blackhole.SetActive(true);
            hasira.SetActive(true);
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
            blackhole.SetActive(false);
            hasira.SetActive(false);
        }
    }
}
