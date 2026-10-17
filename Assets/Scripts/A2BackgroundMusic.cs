using UnityEngine;

public class A2BackgroundMusic : MonoBehaviour
{
    private static A2BackgroundMusic activeMusic;

    public AudioClip themeClip;
    [Range(0f, 1f)] public float volume = 0.14f;

    private AudioSource source;

    public static void EnsurePlaying()
    {
        if (activeMusic == null) return;
        if (activeMusic.source == null)
            activeMusic.source = activeMusic.GetComponent<AudioSource>();
        if (activeMusic.source == null || activeMusic.themeClip == null) return;
        if (activeMusic.source.clip != activeMusic.themeClip)
            activeMusic.source.clip = activeMusic.themeClip;
        activeMusic.source.loop = true;
        if (!activeMusic.source.isPlaying)
            activeMusic.source.Play();
    }

    private void Awake()
    {
        if (activeMusic != null && activeMusic != this)
        {
            Destroy(gameObject);
            return;
        }

        activeMusic = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        if (source == null)
            source = gameObject.AddComponent<AudioSource>();

        source.clip = themeClip;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;
        if (themeClip != null && !source.isPlaying)
            source.Play();
    }

    private void OnDestroy()
    {
        if (activeMusic == this)
            activeMusic = null;
    }
}
