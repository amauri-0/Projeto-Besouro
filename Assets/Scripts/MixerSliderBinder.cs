using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MixerSliderBinder : MonoBehaviour
{
    public enum Type { Music, SFX }
    public Type sliderType;

    private Slider _slider;

    void Awake()
    {
        _slider = GetComponent<Slider>();
        if (AudioManager.Instance == null) return;

        // 1. Inicializa com o percent salvo
        string prefKey = sliderType == Type.Music ? "MusicVolumePercent" : "SFXVolumePercent";
        float init = PlayerPrefs.GetFloat(prefKey, 1f);
        _slider.value = init;

        // 2. Quando eu mexer, atualiza o manager
        _slider.onValueChanged.AddListener(v =>
        {
            if (sliderType == Type.Music)
                AudioManager.Instance.SetMusicVolume(v);
            else
                AudioManager.Instance.SetSFXVolume(v);
        });

        // 3. Quando o manager mudar em outra cena, atualiza o slider
        if (sliderType == Type.Music)
            AudioManager.Instance.OnMusicVolumeChanged += v =>
            { if (_slider.value != v) _slider.value = v; };
        else
            AudioManager.Instance.OnSFXVolumeChanged += v =>
            { if (_slider.value != v) _slider.value = v; };
    }
}
