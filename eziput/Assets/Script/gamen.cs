using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gamen : MonoBehaviour
{
    [SerializeField] private Image brightnessOverlay;
    [SerializeField] private Slider gamenSlider;

    // Start is called before the first frame update
    void Start()
    {
        //スライダー値変更時に明るさを更新
        gamenSlider.onValueChanged.AddListener(SetBrightness);

        //初期値を反映
        SetBrightness(gamenSlider.value);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetBrightness(float value)
    {
        //valueが0=明るい,1=暗いになるようにする
        Color color = brightnessOverlay.color;
        color.a = value; //a値をスライダー値に
        brightnessOverlay.color = color;
    }
}
