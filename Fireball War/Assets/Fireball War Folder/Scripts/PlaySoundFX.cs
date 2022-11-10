using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundFX : MonoBehaviour
{
    [SerializeField]
    AudioSource _audioSource;

    public void SetAudioClip(AudioClip clip)
    {
        _audioSource.clip = clip;
        this.gameObject.SetActive(true);
        DestroyAfterDone();
    }

    void DestroyAfterDone()
    {
        Destroy(this.gameObject, _audioSource.clip.length);
    }
}
