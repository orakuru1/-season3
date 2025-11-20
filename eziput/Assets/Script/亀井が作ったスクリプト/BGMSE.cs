using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class BGMSE : MonoBehaviour
{
    [Header("AudioMixer設定")]
    [SerializeField] AudioMixer audioMixer;

    [Header("AudioSource設定")]
    [SerializeField] AudioSource seAudioSource;
    [SerializeField] AudioSource bgmAudioSource;

    [Header("スライダー設定")]
    [SerializeField] Slider seSlider;
    [SerializeField] Slider bgmSlider;

    [Header("ミュートボタン設定")]
    [SerializeField] Button bgmMuteButton;
    [SerializeField] Button seMuteButton;
    [SerializeField] Sprite soundOnIcon;   // ミュート解除時のアイコン
    [SerializeField] Sprite soundOffIcon;  // ミュート時のアイコン

    private bool isBgmMuted = false;
    private bool isSeMuted = false;

    private float savedBgmVolume = 1f;
    private float savedSeVolume = 1f;

    void Start()
    {
        // スライダー初期値設定
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        seSlider.value = PlayerPrefs.GetFloat("SEVolume", 1f);

        ApplyVolume("BGM", bgmSlider.value);
        ApplyVolume("SE", seSlider.value);

        // リスナー登録
        bgmSlider.onValueChanged.AddListener((value) => {
            if (!isBgmMuted)
            {
                ApplyVolume("BGM", value);
                savedBgmVolume = value;
            }
            PlayerPrefs.SetFloat("BGMVolume", value);
        });

        seSlider.onValueChanged.AddListener((value) => {
            if (!isSeMuted)
            {
                ApplyVolume("SE", value);
                savedSeVolume = value;
            }
            PlayerPrefs.SetFloat("SEVolume", value);
        });

        // ミュートボタンイベント登録
        if (bgmMuteButton != null) bgmMuteButton.onClick.AddListener(ToggleBgmMute);
        if (seMuteButton != null) seMuteButton.onClick.AddListener(ToggleSeMute);

        UpdateMuteIcons();
    }

    void ApplyVolume(string parameterName, float value)
    {
        value = Mathf.Clamp01(value);
        float decibel = (value > 0) ? 20f * Mathf.Log10(value) : -80f;
        decibel = Mathf.Clamp(decibel, -80f, 0f);
        audioMixer.SetFloat(parameterName, decibel);
    }

    void ToggleBgmMute()
    {
        isBgmMuted = !isBgmMuted;

        if (isBgmMuted)
        {
            audioMixer.SetFloat("BGM", -80f);
        }
        else
        {
            ApplyVolume("BGM", savedBgmVolume);
        }

        UpdateMuteIcons();
    }

    void ToggleSeMute()
    {
        isSeMuted = !isSeMuted;

        if (isSeMuted)
        {
            audioMixer.SetFloat("SE", -80f);
        }
        else
        {
            ApplyVolume("SE", savedSeVolume);
        }

        UpdateMuteIcons();
    }

    void UpdateMuteIcons()
    {
        if (bgmMuteButton != null)
        {
            Image img = bgmMuteButton.GetComponent<Image>();
            if (img != null) img.sprite = isBgmMuted ? soundOffIcon : soundOnIcon;
        }

        if (seMuteButton != null)
        {
            Image img = seMuteButton.GetComponent<Image>();
            if (img != null) img.sprite = isSeMuted ? soundOffIcon : soundOnIcon;
        }
    }

    // 🔊 SEテスト再生ボタン用
    public void Sebutton()
    {
        if (seAudioSource != null && !isSeMuted)
        {
            seAudioSource.Play();
        }
    }
}
