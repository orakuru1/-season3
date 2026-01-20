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
    public Text swordExpToNextLevel;

    //[Header("Spear")]
    //public Text spearLvText;

    //[Header("Bow")]
    //public Text BowLvText;

    [Header("Fist")]
    public Text fistLvText;
    public Text fistExp;
    public Text fistExpToNextLevel;

    void Awake()
    {
        player = PlayerUnit.Instance;
    }
    void OnEnable()
    {
        if(player == null) player = PlayerUnit.Instance;

        if(player != null)
        {
            player.OnWeaponMasteryChanged += UpdateDisplay;
            UpdateDisplay();
        }
    }

    void OnDisable()
    {
        if(player != null)
        {
            player.OnWeaponMasteryChanged -= UpdateDisplay;
        }
        
    }

    public void UpdateDisplay()
    {
        if(player == null) return;

        WeaponMasteryProgress sword = player.weaponMasterySet.Get(WeaponType.Sword);
        WeaponMasteryProgress fist = player.weaponMasterySet.Get(WeaponType.None);

        int swordremainExp = sword.ExpToNextLevel - sword.exp;
        int fistremainExp = fist.ExpToNextLevel - fist.exp;

        swordLvText.text = "Lv:" + sword.level;
        swordExp.text = "Exp:" + sword.exp;
        swordExpToNextLevel.text = "次のレベルまで:" + swordremainExp;

        fistLvText.text = "Lv:" + fist.level;
        fistExp.text = "Exp:" + fist.exp;
        fistExpToNextLevel.text = "次のレベルまで:" + fistremainExp;
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
