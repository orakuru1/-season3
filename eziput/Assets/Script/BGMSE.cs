using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class BGMSE : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] AudioSource seAudioSource;
    [SerializeField] AudioSource bgmAudioSource;
    [SerializeField] Slider seSlider;
    [SerializeField] Slider bgmSlider;

    // Start is called before the first frame update
    private void Start()
    {
        //スライダーを触ったら音量が変化する
        bgmSlider.onValueChanged.AddListener((value) =>
        {
            value = Mathf.Clamp01(value);

            //変化するのは-80～0までの間
            float decibel = 20f * Mathf.Log10(value);
            decibel = Mathf.Clamp(decibel, -80f, 0f);
            audioMixer.SetFloat("BGM", decibel);
        });

        //スライダーを触ったら音量が変化する
        seSlider.onValueChanged.AddListener((value) =>
        {
            value = Mathf.Clamp01(value);

            //変化するのは-80～0までの間
            float decibel = 20f * Mathf.Log10(value);
            decibel = Mathf.Clamp(decibel, -80f, 0f);
            audioMixer.SetFloat("SE", decibel);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Sebutton()
    {
        
    }
}
