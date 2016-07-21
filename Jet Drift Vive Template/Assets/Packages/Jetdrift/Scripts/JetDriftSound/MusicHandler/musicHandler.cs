using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class musicHandler : MonoBehaviour
{
    [Header("    JetDriftSound Module")]
    [Space]
    [Space]
    [Space]
    //Themes
    public int initialActiveTheme = 0;
    public int initialActiveTrack = 0;
    public bool playOnAwake = true;
    public float crossfadeTime = 7.0f;
    public List<Theme> themes = new List<Theme>();



    private Theme activeTheme;
    private List<AudioSource> AudioChannels = new List<AudioSource>();
    private AudioSource activeAudioChannel;
    private AudioSource crossfadeAudioChannel;
    private bool intoCrossfadeAudioChannel = true;

    private static musicHandler music;
    public static musicHandler Music
    {
        get
        {
            if (music == null)
            {
                GameObject MusicPrefab = new GameObject("MusicPrefab");
                MusicPrefab.AddComponent<musicHandler>();
            }
            return music;
        }
    }

    void Awake()
    {
        if (music == null)
        {
            DontDestroyOnLoad(gameObject);                                  //makes gameObject persist between level loads
            music = this;                                                    //sets this instance as the only instance
        }
        else if (music != this)
        {
            Destroy(gameObject);                                              //if its not the only instance, destroy it
        }
        activeTheme = themes[initialActiveTheme];
        foreach (AudioSource A in gameObject.GetComponents<AudioSource>())
        {
            AudioChannels.Add(A);
        }
        activeAudioChannel = AudioChannels[0];
        crossfadeAudioChannel = AudioChannels[1];

        if (playOnAwake)
        {
            play(initialActiveTrack);
        }
    }

    // Use this for initialization
    void Start ()
    {
        
	}

    /// <summary>
    /// plays a track in the current ActiveTheme
    /// </summary>
    /// <param name="index">index of the track</param>
    public static void play(int index)                                            
    {
        Music.activeAudioChannel.clip = Music.activeTheme.tracks[index];
        Music.activeAudioChannel.Play();
    }

    /// <summary>
    /// plays a track in a certain theme
    /// </summary>
    /// <param name="theme">index of theme, will change ActiveTheme</param>
    /// <param name="track">index of track</param>
    public static void play(int theme, int track)
    {
        Music.activeTheme = Music.themes[theme];
        Music.activeAudioChannel.clip = Music.activeTheme.tracks[track];
        Music.activeAudioChannel.Play();
    }

    public static void play(string trackName)
    {
        Music.activeAudioChannel.clip = Music.activeTheme.tracks.Find(x => x.name == trackName);
        Music.activeAudioChannel.Play();
    }


    public static void changeTheme(int theme)
    {
        Music.activeTheme = Music.themes[theme];
    }

    public static void playCrossfade(int index)
    {
        Music.intoCrossfadeAudioChannel = !Music.intoCrossfadeAudioChannel;
        Music.crossfadeAudioChannel.clip = Music.activeTheme.tracks[index];
        Music.crossfadeAudioChannel.Play();
        Music.crossfadeAudioChannel.volume = 0;
        Music.StartCoroutine("crossfade");
    }


    IEnumerator crossfade()
    {
        for(;;)
        { 
            Music.activeAudioChannel.volume -= 1/(2*crossfadeTime);
            Music.crossfadeAudioChannel.volume += 1/(2*crossfadeTime);
            if (Music.activeAudioChannel.volume == 0.0f && Music.crossfadeAudioChannel.volume == 1.0f)
            {
                AudioSource temp = new AudioSource();
                temp = Music.crossfadeAudioChannel;
                Music.crossfadeAudioChannel = Music.activeAudioChannel;                                 //swap active and crossfade back now that you've crossfaded
                Music.activeAudioChannel = temp;
                StopCoroutine("crossfade");
            }
            yield return new WaitForSeconds(1/(2*crossfadeTime));
        }
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////
    ///BEGIN UNITY INSPECTOR INTERFACE

    /// <summary>
    /// Play Theme 0
    /// </summary>
    [ContextMenu("Play Theme 0")]
    void UIPlayTheme0()
    {
        musicHandler.playCrossfade(0);
    }
    /// <summary>
    /// Play Theme 1 
    /// </summary>
    [ContextMenu("Play Theme 1")]
    void UIPlayTheme1()
    {
        musicHandler.playCrossfade(1);
    }

}
