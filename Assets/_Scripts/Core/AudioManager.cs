using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // Компоненты для воспроизведения, создаваемые кодом
    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioMixer audioMixer;

    // Константы параметров микшера
    private const string MASTER_VOLUME_PARAM = "MasterVolume";
    private const string MUSIC_VOLUME_PARAM = "MusicVolume";
    private const string SFX_VOLUME_PARAM = "SFXVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Инициализируем аудио-составляющую
        InitializeAudioComponents();
    }

    private void Start()
    {
        // Загружаем сохраненные игроком настройки громкости
        LoadVolumeSettings();

        // Подписываемся на события состояний игры из EventBus
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnGamePaused += HandleGamePaused;
            EventBus.Instance.OnGameResumed += HandleGameResumed;
        }
    }

    private void InitializeAudioComponents()
    {
        // 1. Динамически добавляем AudioSource для музыки и звуков
        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        // Настраиваем базовые параметры (например, музыка должна повторяться)
        musicSource.loop = true;

        // 2. Загружаем AudioMixer из папки Assets/Resources/
        // Убедитесь, что ваш микшер лежит по пути: Assets/Resources/MainAudioMixer.mixer
        audioMixer = Resources.Load<AudioMixer>("MainAudioMixer");

        if (audioMixer != null)
        {
            // Находим нужные группы в микшере и привязываем к ним наши AudioSource
            AudioMixerGroup[] musicGroups = audioMixer.FindMatchingGroups("Music");
            AudioMixerGroup[] sfxGroups = audioMixer.FindMatchingGroups("SFX");

            if (musicGroups.Length > 0) musicSource.outputAudioMixerGroup = musicGroups[0];
            if (sfxGroups.Length > 0) sfxSource.outputAudioMixerGroup = sfxGroups[0];
        }
        else
        {
            Debug.LogWarning("AudioManager: Файл 'MainAudioMixer' не найден в папке Assets/Resources/. " +
                             "Управление группами микшера через слайдеры работать не будет, пока вы его не добавите.");
        }
    }


    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Попытка воспроизвести звуковой эффект с пустым AudioClip.");
            return;
        }

        sfxSource.PlayOneShot(clip);
    }


    // ==========================================
    // РЕАКЦИЯ НА СОСТОЯНИЯ ГЕЙМ-МЕНЕДЖЕРА
    // ==========================================

    private void HandleGamePaused()
    {
        Debug.Log("AudioManager: Игра приостановлена. (Можно приглушить музыку или поставить на паузу SFX)");
        // Пример: Ставим на паузу все текущие эффекты, чтобы они не зависали в воздухе при паузе
        sfxSource.Pause();
    }

    private void HandleGameResumed()
    {
        Debug.Log("AudioManager: Игра возобновлена. (Возвращаем звук)");
        sfxSource.UnPause();
    }

    // ==========================================
    // УПРАВЛЕНИЕ ГРОМКОСТЬЮ ДЛЯ СЛАЙДЕРОВ
    // ==========================================

    private void LoadVolumeSettings()
    {
        SetMasterVolume(PlayerPrefs.GetFloat("SavedMasterVolume", 0.75f));
        SetMusicVolume(PlayerPrefs.GetFloat("SavedMusicVolume", 0.75f));
        SetSFXVolume(PlayerPrefs.GetFloat("SavedSFXVolume", 0.75f));
    }

    public void SetMasterVolume(float value)
    {
        float clampedValue = Mathf.Clamp(value, 0.0001f, 1f);
        float dB = Mathf.Log10(clampedValue) * 20;
        if (audioMixer != null) audioMixer.SetFloat(MASTER_VOLUME_PARAM, dB);
        PlayerPrefs.SetFloat("SavedMasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        float clampedValue = Mathf.Clamp(value, 0.0001f, 1f);
        float dB = Mathf.Log10(clampedValue) * 20;
        if (audioMixer != null) audioMixer.SetFloat(MUSIC_VOLUME_PARAM, dB);
        PlayerPrefs.SetFloat("SavedMusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        float clampedValue = Mathf.Clamp(value, 0.0001f, 1f);
        float dB = Mathf.Log10(clampedValue) * 20;
        if (audioMixer != null) audioMixer.SetFloat(SFX_VOLUME_PARAM, dB);
        PlayerPrefs.SetFloat("SavedSFXVolume", value);
    }

    private void OnDestroy()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnGamePaused -= HandleGamePaused;
            EventBus.Instance.OnGameResumed -= HandleGameResumed;
        }
    }
}