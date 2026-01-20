using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance;

    [Header("Status")]
    public int maxHP = 20;
    public int currentHP = 20;
    public int attack = 5;
    public int defense = 2;
    public List<GodData> ownedGods = new List<GodData>();

    [Header("Progress")]
    public int currentStage = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
