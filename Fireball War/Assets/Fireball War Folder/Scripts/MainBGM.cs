using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainBGM : MonoBehaviour
{
    [SerializeField]
    AudioSource _audioSource;

    public static MainBGM Instance { get; private set; }
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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if(_audioSource.isPlaying)
                _audioSource.Stop();
            else
                _audioSource.Play();
        }

        if(Time.timeScale == 0 && _audioSource.isPlaying)
        {
            _audioSource.Pause();
        }
        else if(Time.timeScale != 0 && !_audioSource.isPlaying)
        {
            _audioSource.UnPause();
        }
    }
}
