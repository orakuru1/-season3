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

    [Header("Axe")]
    public Text axeLvText;
    public Text axeExp;
    public Text axeExpToNextLevel;

    [Header("Spear")]
    public Text spearLvText;
    public Text spearExp;
    public Text spearExpToNextLevel;

    [Header("Bow")]
    public Text bowLvText;
    public Text bowExp;
    public Text bowExpToNextLevel;

    [Header("Staff")]
    public Text staffLvText;
    public Text staffExp;
    public Text staffExpToNextLevel;

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
        WeaponMasteryProgress axe = player.weaponMasterySet.Get(WeaponType.Axe);
        WeaponMasteryProgress spear = player.weaponMasterySet.Get(WeaponType.Spear);
        WeaponMasteryProgress bow = player.weaponMasterySet.Get(WeaponType.Bow);
        WeaponMasteryProgress staff = player.weaponMasterySet.Get(WeaponType.Staff);
        WeaponMasteryProgress fist = player.weaponMasterySet.Get(WeaponType.None);

        int swordremainExp = sword.ExpToNextLevel - sword.exp;
        int axeremainExp = axe.ExpToNextLevel - axe.exp;
        int spearremainExp = spear.ExpToNextLevel - spear.exp;
        int bowremainExp = bow.ExpToNextLevel - bow.exp;
        int staffremainExp = staff.ExpToNextLevel - staff.exp;
        int fistremainExp = fist.ExpToNextLevel - fist.exp;

        //SwordType
        swordLvText.text = "Lv:" + sword.level;
        swordExp.text = "Exp:" + sword.exp;
        swordExpToNextLevel.text = "次のレベルまで:" + swordremainExp;

        //AxeType
        axeLvText.text = "Lv:" + axe.level;
        axeExp.text = "Exp:" + axe.exp;
        axeExpToNextLevel.text = "次のレベルまで:" + axeremainExp;

        //SpearType
        spearLvText.text = "Lv:" + spear.level;
        spearExp.text = "Exp:" + spear.exp;
        spearExpToNextLevel.text = "次のレベルまで:" + spearremainExp;

        //BowType
        bowLvText.text = "Lv:" + bow.level;
        bowExp.text = "Exp:" + bow.exp;
        bowExpToNextLevel.text = "次のレベルまで:" + swordremainExp;

        //StaffType
        staffLvText.text = "Lv:" + staff.level;
        staffExp.text = "Exp:" + staff.exp;
        staffExpToNextLevel.text = "次のレベルまで:" + staffremainExp;

        //FistType
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
