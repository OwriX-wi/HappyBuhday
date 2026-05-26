using UnityEngine;
using System;

public class EventBus : MonoBehaviour
{
    public static EventBus Instance { get; private set; }

    public event System.Action<Vector3> OnPlayerHitFurniture;
    public event System.Action OnPlayerStartedRunning;

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

    public void TriggerPlayerHitFurniture(Vector3 hitPosition)
    {
        OnPlayerHitFurniture?.Invoke(hitPosition);
    }

    public void TriggerPlayerStartedRunning()
    {
        OnPlayerStartedRunning?.Invoke();
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
