using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GodInventoryUI : MonoBehaviour
{
    public static GodInventoryUI Instance;
    public Image[] godIcons = new Image[4];

    private void Awake()
    {
        Instance = this;
    }
    
    public void UpdateInventoryGodIcons(List<GodData> gods)
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
                godIcons[i].color = new Color(1,1,1,0);
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
