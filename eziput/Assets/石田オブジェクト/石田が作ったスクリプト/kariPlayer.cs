using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kariPlayer : MonoBehaviour
{
    public StatusSaveData Status = new StatusSaveData();
    [SerializeField] private SaveLoad saveLoad;
    void Start()
    {
        saveLoad.players.Add(this);
        Status.idname = this.name;
        Status.characterName = "勇者";
        Status.HP = 100;
        Status.maxHP = 100;
        Status.speed = 5.0f;
    }

    // Update is called once per frame
    void Update()
    {
    
    }
}
