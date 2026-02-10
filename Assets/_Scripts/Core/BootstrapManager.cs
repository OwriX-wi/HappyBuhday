using UnityEngine;

public class BootstrapManager : MonoBehaviour
{
    private static bool _initialized = false;

    private void Awake()
    {
        if (_initialized)
        {
            Destroy(gameObject);
            return;
        }

        _initialized = true;
        DontDestroyOnLoad(gameObject);

        CreateIfNotExists<GameManager>("GameManager");
        CreateIfNotExists<SceneLoader>("SceneLoader");
        CreateIfNotExists<EventBus>("EventBus");
        CreateIfNotExists<InputManager>("InputManager");

        SceneLoader.Instance.Load(SceneNames.MainMenu);
    }

    private void CreateIfNotExists<T>(string objectName) where T : Component
    {
        T existing = FindFirstObjectByType<T>();
        if (existing != null)
        {
            DontDestroyOnLoad(existing.gameObject);
            return;
        }
        GameObject go = new GameObject(objectName);
        T component = go.AddComponent<T>();
        DontDestroyOnLoad(go);
    }
}