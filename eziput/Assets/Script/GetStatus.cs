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

    // Start is called before the first frame update
    void Start()
    {
        if(playerUnit == null)
        {
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
        AttackText.text = $"ATK: {playerUnit.status.attack}";
        DefenceText.text = $"DEF: {playerUnit.status.defense}";
        SpeedText.text = $"Speed: {playerUnit.status.speed}";
        soubiText.text = "装備中";
        bukisoubiText.text = "武器";
        bougusoubiText.text = "防具";

    }
}
