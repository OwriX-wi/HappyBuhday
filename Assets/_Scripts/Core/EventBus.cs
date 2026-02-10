using UnityEngine;
using System;

public class EventBus : MonoBehaviour
{
    public static EventBus Instance { get; private set; }

    //sobitiya
    public event Action OnGameResumed;
    public event Action OnGamePaused;


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

    public void RaiseGameResumed()
    {
        OnGameResumed?.Invoke();
    }

    public void RaiseGamePaused()
    {
        OnGamePaused?.Invoke();
    }
}
