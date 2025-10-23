using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SEManager : MonoBehaviour
{
    public static SEManager Instance;

    [SerializeField] private AudioSource Sesource; //SE用AudioSource
    [SerializeField] private AudioClip buttonClip; //ボタン押下SE

    private void Awake()
    {
        //シングルトンにして他スクリプトから呼べるように
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayButtonSE()
    {
        Sesource.PlayOneShot(buttonClip);
    }

    public void SetSEVolume(float volume)
    {
        Sesource.volume = volume;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
