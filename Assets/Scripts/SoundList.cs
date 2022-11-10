using UnityEngine;

public class SoundList : MonoBehaviour
{
    

    public static SoundList instance;
    private void Awake()
    {
        instance = this;
    }

    #region Sound References

    public AudioClip testSound;
    public System.Collections.Generic.List<AudioClip> list_footsteps = new System.Collections.Generic.List<AudioClip>();

    #endregion


    //public AudioClip GetSound(SoundData b)
    //{
    //    return b.Get();
    //}
}


//public class SoundData
//{
//    private AudioClip sound;
//    private string filepath;
//    public AudioClip Get()
//    {//load on initialization/demand.
//        if (sound == null)
//        {
//            sound = (AudioClip)Resources.Load(filepath);
//        }
//        Debug.Log("GetSound(): returned sound " + sound.name);
//        return sound;
//    }
//    public SoundData(string fl)
//    {
//        filepath = fl;
//    }
//}


//class s_fire_ak47 : SoundData
//{
//    public s_fire_ak47(string fl) : base(fl)
//    {

//    }
//}

