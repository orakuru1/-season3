using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class BossUIManeger : MonoBehaviour
{
    public static BossUIManeger Instance { get; private set; }
    public Slider BossSliderPrefab;
    public Transform BossSliderPosition;
    [SerializeField]private Unit unit;

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    private void Start()
    {
        
    }

    public Slider Initialize(Unit bossUnit)
    {
        unit = bossUnit;
        Slider BossSliderInstance = Instantiate(BossSliderPrefab, BossSliderPosition.position, Quaternion.identity, BossSliderPosition);
        BossSliderInstance.gameObject.SetActive(false);
        return BossSliderInstance;
    }


}
