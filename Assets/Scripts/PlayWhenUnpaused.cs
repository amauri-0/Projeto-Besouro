using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayWhenUnpaused : MonoBehaviour
{
    private AudioSource _audio;
    private float _lastTimeScale;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        float ts = Time.timeScale;
        // se mudou desde o último frame...
        if (ts != _lastTimeScale)
        {
            if (ts == 1f)
            {
                // de “pausado” pra “rodando”:
                if (!_audio.isPlaying)
                    _audio.UnPause();
            }
            else
            {
                // de “rodando” pra “pausado”:
                if (_audio.isPlaying)
                    _audio.Pause();
            }
            _lastTimeScale = ts;
        }
    }
}
