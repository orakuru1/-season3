using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GodUIManager : MonoBehaviour
{
    public static GodUIManager Instance;

    //ここに4つの画像を登録（インスペクタで設定）
    public Image[] godIcons = new Image[4];

    private void Awake()
    {
        Instance = this;
    }
    public void UpdateGodIcons(List<GodData> gods)
    {
        for(int i = 0; i < godIcons.Length; i++)
        {
            if(i < gods.Count)
            {
                godIcons[i].sprite = gods[i].icon;
                godIcons[i].color = Color.white;
            }
            else
            {
                godIcons[i].sprite = null;
                godIcons[i].color = new Color(1,1,1,0); //透明化
            }
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
