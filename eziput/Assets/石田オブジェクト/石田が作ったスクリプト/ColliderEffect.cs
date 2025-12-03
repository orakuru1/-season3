using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderEffect : MonoBehaviour
{
    public GodTrial godTrial;//生成するときに、どの試練かを入れる
    private void OnTriggerEnter(Collider other)
    {
        Unit unit = other.GetComponent<Unit>();
        if(unit != null && godTrial != null)
        {
            GodTrialManeger.Instance.TriggerGodTrial(godTrial, unit);
            Debug.Log("ColliderEffectが衝突を検出しました");
        }
    }


}
