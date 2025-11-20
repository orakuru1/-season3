using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sceneseni : MonoBehaviour
{
    private bool firstPush = false; //スタート
    private bool returnPush = false; //戻る

    // Start is called before the first frame update

    //スタートボタンを押したらゲーム画面へ
    public void PressStart()
    {
        if(!firstPush)
        {
            SceneManager.LoadScene("kamei");
            firstPush = true;
        }
    }

    public void PressRetrun()
    {
        if(!returnPush)
        {
            SceneManager.LoadScene("title");
            returnPush = true;
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
