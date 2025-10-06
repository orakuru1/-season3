using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCanvas : MonoBehaviour
{
    public GameObject panel;
    // Start is called before the first frame update
    void Start()
    {
        panel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //ESCキーが押されたトグル
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("押されてない");
            TogglePanel();
        }
    }

    public void ShowPanel()
    {
        panel.SetActive(true);
    }

    public void HisPanel()
    {
        panel.SetActive(false);
    }

    public void TogglePanel()
    {
        panel.SetActive(!panel.activeSelf);
    }
}
