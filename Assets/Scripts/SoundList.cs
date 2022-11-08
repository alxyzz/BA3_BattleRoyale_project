using UnityEngine;

public class SoundList : MonoBehaviour
{
    /* stores empty references to sounds, then initializes them if they are used while simultaneously loading in the sound file itself on demand on the first time
     * therefore we do not load in sounds that we are not using or have not used yet, sounds are loaded in clientside the first time that a sound is played. 
     * in the future could also do a check if we are too far from the sound because 
     * working as it does right now, any sound on the map even inaudible would theoretically 
     * still load the respective sound
     */

    public static SoundList instance;
    private void Awake()
    {
        instance = this;
    }

    #region Sound References

    private SoundData testSound   = new SoundData("Resources/Sounds/missing");

    #endregion


    public AudioClip GetSound(SoundData b)
    {
        return b.Get();
    }
}


public class SoundData
{
    private AudioClip sound;
    private string filepath;
    public AudioClip Get()
    {
        if (sound == null)
        {
            sound = (AudioClip)Resources.Load(filepath);
        }
        Debug.Log("GetSound(): returned sound " + sound.name);
        return sound;
    }
    public SoundData(string fl)
    {//load on initialization. Filepath is determined already
        filepath = fl;
    }
}


class s_fire_ak47 : SoundData
{
    public s_fire_ak47(string fl) : base(fl)
    {

    }
}

