using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;

public class sliderToMixerLevel : MonoBehaviour
{

    public string property;
    public AudioMixer masterMixer;

    private float VolumeSliderGet;
    private float setValue;

    private float currentVol;

    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(masterMixer);
    }

    // Update is called once per frame
    void Update()
    {
        //VolumeSliderGet = gameObject.GetComponent<Slider>().value;
        //setValue = (VolumeSliderGet - 1) * 80;
        //masterMixer.GetFloat(property, out currentVol);
        //Debug.Log(currentVol);

        //Debug.Log(masterMixer.SetFloat("TEST1", setValue));
    }
}