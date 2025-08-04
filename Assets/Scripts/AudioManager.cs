using UnityEngine;
using UnityEngine.Audio;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Arraste aqui seu AudioMixer asset")]
    public AudioMixer mixer;

    public event Action<float> OnMusicVolumeChanged;
    public event Action<float> OnSFXVolumeChanged;

    const string MUSIC_PARAM = "MusicVol";
    const string SFX_PARAM = "SFXVol";
    const string MUSIC_PREF = "MusicVolumePercent";
    const string SFX_PREF = "SFXVolumePercent";

    // Limite mínimo de dB quando percent = 0
    private const float MIN_DB = -80f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Carrega percentuais salvos ou assume 1 (100%)
            float musicPercent = PlayerPrefs.GetFloat(MUSIC_PREF, 1f);
            float sfxPercent = PlayerPrefs.GetFloat(SFX_PREF, 1f);

            // Aplica sem disparar eventos
            ApplyMusicVolume(musicPercent, invokeEvent: false);
            ApplySFXVolume(sfxPercent, invokeEvent: false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Recebe percent (0–1), converte para dB via 20*log10(percent), e aplica.
    /// </summary>
    public void SetMusicVolume(float percent)
    {
        percent = Mathf.Clamp01(percent);
        PlayerPrefs.SetFloat(MUSIC_PREF, percent);
        PlayerPrefs.Save();
        ApplyMusicVolume(percent, invokeEvent: true);
    }

    public void SetSFXVolume(float percent)
    {
        percent = Mathf.Clamp01(percent);
        PlayerPrefs.SetFloat(SFX_PREF, percent);
        PlayerPrefs.Save();
        ApplySFXVolume(percent, invokeEvent: true);
    }

    private void ApplyMusicVolume(float percent, bool invokeEvent)
    {
        float dB = percent > 0f ? 20f * Mathf.Log10(percent) : MIN_DB;
        mixer.SetFloat(MUSIC_PARAM, dB);
        if (invokeEvent)
            OnMusicVolumeChanged?.Invoke(percent);
    }

    private void ApplySFXVolume(float percent, bool invokeEvent)
    {
        float dB = percent > 0f ? 20f * Mathf.Log10(percent) : MIN_DB;
        mixer.SetFloat(SFX_PARAM, dB);
        if (invokeEvent)
            OnSFXVolumeChanged?.Invoke(percent);
    }
}