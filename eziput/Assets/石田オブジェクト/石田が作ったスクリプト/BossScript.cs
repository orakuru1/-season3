using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossScript : MonoBehaviour
{
    private Unit unit;
    private Slider BossSliderInstance;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        OnBossSliderTrue();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 逃げたら消す（好み）
        OnBossSliderFalse();
    }

    public void OnBossSliderTrue()
    {
        BossSliderInstance.gameObject.SetActive(true);
    }
    public void OnBossSliderFalse()
    {
        BossSliderInstance.gameObject.SetActive(false);
    }

    private void Start()
    {
        unit = GetComponent<Unit>();
        BossSliderInstance = BossUIManeger.Instance.Initialize(GetComponent<Unit>());
        unit.hpSlider = BossSliderInstance;
    }

}
