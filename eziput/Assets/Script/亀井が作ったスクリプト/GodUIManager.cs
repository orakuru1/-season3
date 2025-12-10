using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GodUIManager : MonoBehaviour
{
    public static GodUIManager Instance;

    //ここに4つの画像を登録（インスペクタで設定）
    public Image[] godIcons = new Image[4];
    public Image[] cooldownMasks; 

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

    public void UpdateCooldownUI(List<GodData> ownedGods)
    {
        for(int i = 0; i < cooldownMasks.Length; i++)
        {
            if(i < ownedGods.Count)
            {
                GodData god = ownedGods[i];

                if(god.abilities.floatcurrentCooldown > 0)
                {
                    cooldownMasks[i].gameObject.SetActive(true);
                    cooldownMasks[i].fillAmount = god.abilities.floatcurrentCooldown / god.abilities.cooldown;
                }
                else
                {
                    cooldownMasks[i].gameObject.SetActive(false);
                }  
            }
            else
            {
                cooldownMasks[i].gameObject.SetActive(false);
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
