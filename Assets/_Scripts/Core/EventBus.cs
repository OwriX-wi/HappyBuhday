using UnityEngine;
using System;

public class EventBus : MonoBehaviour
{
    public static EventBus Instance { get; private set; }

    public event System.Action<Vector3> OnPlayerHitFurniture;
    public event System.Action OnPlayerStartedRunning;

    // Новое событие: позиция источника звука и длительность в секундах
    public event Action<Vector3, float> OnPlayerMadeSound;

    //sobitiya
    public event Action OnGameResumed;
    public event Action OnGamePaused;
    public event Action OnGameSetOpened;
    public event Action OnGameReturned;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Поддерживается прежний вызов; теперь можно передать duration (по умолчанию 1s)
    public void TriggerPlayerHitFurniture(Vector3 hitPosition, float duration = 1f)
    {
        OnPlayerHitFurniture?.Invoke(hitPosition);
        OnPlayerMadeSound?.Invoke(hitPosition, duration);
    }

    // Триггер старта бега — считаем как громкий/длинный звук (duration по умолчанию 3s)
    public void TriggerPlayerStartedRunning()
    {
        OnPlayerStartedRunning?.Invoke();
        // Если нужно, можно изменить длительность в другом месте
        OnPlayerMadeSound?.Invoke(Vector3.zero, 3f);
    }

    // Прямой триггер для произвольного звука (позиция + длительность)
    public void TriggerPlayerMadeSound(Vector3 position, float duration, bool isRunning)
    {
        OnPlayerMadeSound?.Invoke(position, duration);
    }

    public void RaiseGameSettings()
    {
        OnGameSetOpened?.Invoke();
    }

    public void RaiseGameResumed()
    {
        OnGameResumed?.Invoke();
    }

    public void RaiseGamePaused()
    {
        OnGamePaused?.Invoke();
    }

    public void RaiseGameReturned()
    {
        OnGameReturned?.Invoke();
    }
}
