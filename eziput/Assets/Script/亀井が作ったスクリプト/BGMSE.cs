using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class BGMSE : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;

    [Header("Sliders")]
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider seSlider;

    [Header("Mute Buttons")]
    [SerializeField] Button bgmMuteButton;
    [SerializeField] Button seMuteButton;
    [SerializeField] Sprite soundOnIcon;
    [SerializeField] Sprite soundOffIcon;


    [Header("Audio Sources")]
    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioSource seSource;

    bool bgmMuted;
    bool seMuted;
    float savedBgm = 1f;
    float savedSe = 1f;

    void Start()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        seSlider.value = PlayerPrefs.GetFloat("SEVolume", 1f);

        Apply("BGM", bgmSlider.value);
        Apply("SE", seSlider.value);

        bgmSlider.onValueChanged.AddListener(v =>
        {
            if (!bgmMuted)
            {
                savedBgm = v;
                Apply("BGM", v);
            }
            PlayerPrefs.SetFloat("BGMVolume", v);
        });

        seSlider.onValueChanged.AddListener(v =>
        {
            if (!seMuted)
            {
                savedSe = v;
                Apply("SE", v);
            }
            PlayerPrefs.SetFloat("SEVolume", v);
        });

        bgmMuteButton.onClick.AddListener(ToggleBgmMute);
        seMuteButton.onClick.AddListener(ToggleSeMute);

        UpdateIcons();

        SoundManager.Instance.InitializeSettings(bgmSource, seSource);
    }

    void Apply(string param, float v)
    {
        float db = v > 0 ? 20f * Mathf.Log10(v) : -80f;
        audioMixer.SetFloat(param, Mathf.Clamp(db, -80f, 0f));
    }

    void ToggleBgmMute()
    {
        bgmMuted = !bgmMuted;
        audioMixer.SetFloat("BGM", bgmMuted ? -80f : Db(savedBgm));
        UpdateIcons();
    }

    void ToggleSeMute()
    {
        seMuted = !seMuted;
        audioMixer.SetFloat("SE", seMuted ? -80f : Db(savedSe));
        UpdateIcons();
    }

    float Db(float v) => v > 0 ? 20f * Mathf.Log10(v) : -80f;

    void UpdateIcons()
    {
        bgmMuteButton.image.sprite = bgmMuted ? soundOffIcon : soundOnIcon;
        seMuteButton.image.sprite = seMuted ? soundOffIcon : soundOnIcon;
    }

    public void InitializeSettings(Slider SS, Slider BS, Button sb, Button bb)
    {
        this.seSlider = SS;
        this.bgmSlider = BS;
        this.seMuteButton = sb;
        this.bgmMuteButton = bb;
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }
}
