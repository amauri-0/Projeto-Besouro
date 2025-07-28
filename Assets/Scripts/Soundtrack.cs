using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Soundtrack : MonoBehaviour
{
    private static Soundtrack _instance;
    private AudioSource _audioSource;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = true;
            _audioSource.loop = true;
            if (!_audioSource.isPlaying)
                _audioSource.Play();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}