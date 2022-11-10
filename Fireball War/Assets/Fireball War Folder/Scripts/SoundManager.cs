using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    GameObject soundFxPrefab;
    [SerializeField]
    List<SoundFX> soundFXs = new List<SoundFX>();

    public static SoundManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void PlaySound(SoundFxEnum soundFX)
    {
        PlaySoundFX soundObj = Instantiate(soundFxPrefab.GetComponent<PlaySoundFX>());
        soundObj.SetAudioClip(FindSound(soundFX));
    }

    AudioClip FindSound(SoundFxEnum soundEnum)
    {
        foreach(var sound in soundFXs)
        {
            if(soundEnum == sound.soundEnum)
            {
                return sound.audioClip;
            }
        }

        return null;
    }
}

public enum SoundFxEnum
{
    shootFireball,
    whiff,
    fireballClash,
    justFrame,
    fireballHit,
    AnnounceReady,
    AnnounceShoot
}

[System.Serializable]
public class SoundFX
{
    public SoundFxEnum soundEnum;
    public AudioClip audioClip;
}
