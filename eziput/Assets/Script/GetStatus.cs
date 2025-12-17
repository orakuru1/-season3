using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GetStatus : MonoBehaviour
{
    [SerializeField] private Unit playerUnit; //プレイヤーのunitをセット
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text HPText;
    [SerializeField] private Text AttackText;
    [SerializeField] private Text DefenceText;
    [SerializeField] private Text SpeedText;
    [SerializeField] private Text soubiText;
    [SerializeField] private Text bukisoubiText;
    [SerializeField] private Text bougusoubiText;

    [SerializeField] private Image statusPanel; //ステータス全体

    public string equippedWeaponName = "なし";
    public string equippedArmorName = "なし";

    // Start is called before the first frame update
    void Start()
    {
        if(playerUnit == null)
        {
            //いずれ消せ。統合用では用済み・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・
            playerUnit = FindObjectsOfType<Unit>()?.FirstOrDefault(u => u.team == Unit.Team.Player);
        }
        UpdateStatus();
    }

    // Update is called once per frame
    void Update()
    {
        //UIが開いている間だけ更新
        if(statusPanel != null && statusPanel.gameObject.activeSelf)
        {
            UpdateStatus();
        }
    }

    public void UpdateStatus()
    {
        if(playerUnit == null || playerUnit.status == null) return;

        nameText.text = playerUnit.status.unitName;
        levelText.text = $"Lv. {playerUnit.status.level}";
        HPText.text = $"HP: {playerUnit.status.currentHP}/{playerUnit.status.maxHP}";
        AttackText.text = $"ATK: {playerUnit.status.attack}+{playerUnit.equidpAttackBonus} = {playerUnit.TotalAttack}";
        DefenceText.text = $"DEF: {playerUnit.status.defense}+{playerUnit.equipDefenseBonus} = {playerUnit.Totaldefense}";
        SpeedText.text = $"Speed: {playerUnit.status.speed}";
        soubiText.text = "装備中";
        bukisoubiText.text = $"武器: {equippedWeaponName}";
        bougusoubiText.text = $"防具: {equippedArmorName}";

    }

    public void OnEnable()
    {
        GameManager.OnPlayerSpawned += SetPlayer;
    }
    public void OnDisable()
    {
        GameManager.OnPlayerSpawned -= SetPlayer;
    }
    public void SetPlayer(GameObject playerObj)
    {
        playerUnit = playerObj.GetComponent<Unit>();
    }
}
