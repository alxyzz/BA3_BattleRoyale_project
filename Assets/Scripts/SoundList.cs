using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundList : MonoBehaviour
{


    private static SoundList instance;
    private void Awake()
    {
        if (null != instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    struct namedAudioClip
    {
        public string name;
        public AudioClip soundclip;
    }
    [SerializeField] private List<namedAudioClip> _genericAudioDatabase;
    [SerializeField] private List<AudioClip> _footsteps;
    public static AudioClip GetSound(string soundName)
    {

        return instance._genericAudioDatabase.Where(i => i.name == soundName).FirstOrDefault().soundclip; ;
    }
    public static AudioClip GetRandomFootstep()
    {
        //Debug.Log("got random footstep");
        return instance._footsteps[UnityEngine.Random.Range(0, instance._footsteps.Count - 1)];
    }
}



