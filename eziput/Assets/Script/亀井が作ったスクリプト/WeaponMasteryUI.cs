using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponMasteryUI : MonoBehaviour
{
    public PlayerUnit player;

    [Header("Sword")]
    public Text swordLvText;
    public Text swordExp;

    //[Header("Spear")]
    //public Text spearLvText;

    //[Header("Bow")]
    //public Text BowLvText;

    void OnEnable()
    {
        UpdateDisplay();
        
    }

    public void UpdateDisplay()
    {
        if(player == null) return;

        swordLvText.text = "Lv" + player.weaponMasterySet.Get(WeaponType.Sword).level;
        swordExp.text = "Exp" + player.weaponMasterySet.Get(WeaponType.Sword).exp;
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
