//using UnityEditor.VersionControl;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using System.Reflection;

//public class BootstrapManager : MonoBehaviour
//{
//    private static bool _initialized = false;

//    private void Awake()
//    {
//        if (_initialized)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        _initialized = true;
//        DontDestroyOnLoad(gameObject);

//        CreateIfNotExists<GameManager>("GameManager");
//        CreateIfNotExists<SceneLoader>("SceneLoader");
//        CreateIfNotExists<EventBus>("EventBus");
//        CreateIfNotExists<InputManager>("InputManager");

//        SceneLoader.Instance.Load(SceneNames.MainMenu);
//    }

//    private void CreateIfNotExists<T>(string objectName) where T : Component
//    {
//        T existing = FindFirstObjectByType<T>();
//        if (existing != null)
//        {
//            DontDestroyOnLoad(existing.gameObject);
//            return;
//        }
//        GameObject go = new GameObject(objectName);
//        T component = go.AddComponent<T>();
//        DontDestroyOnLoad(go);
//    }
//}



using UnityEngine;
using UnityEngine.InputSystem;

public class BootstrapManager : MonoBehaviour
{
    private static bool _initialized;

    private void Awake()
    {
        if (_initialized)
        {
            Destroy(gameObject);
            return;
        }

        _initialized = true;
        DontDestroyOnLoad(gameObject);

        // Создаём/поднимаем основные менеджеры
        CreateGameManager();
        CreateSceneLoader();
        CreateEventBus();
        CreateInputManager();

        SceneLoader.Instance.Load(SceneNames.MainMenu);
    }

    private static void CreateGameManager()
    {
        GameManager existing = FindFirstObjectByType<GameManager>();
        if (existing != null)
        {
            DontDestroyOnLoad(existing.gameObject);
            return;
        }

        GameObject go = new GameObject("GameManager");
        go.AddComponent<GameManager>();
        DontDestroyOnLoad(go);
    }

    private static void CreateSceneLoader()
    {
        SceneLoader existing = FindFirstObjectByType<SceneLoader>();
        if (existing != null)
        {
            DontDestroyOnLoad(existing.gameObject);
            return;
        }

        GameObject go = new GameObject("SceneLoader");
        go.AddComponent<SceneLoader>();
        DontDestroyOnLoad(go);
    }

    private static void CreateEventBus()
    {
        EventBus existing = FindFirstObjectByType<EventBus>();
        if (existing != null)
        {
            DontDestroyOnLoad(existing.gameObject);
            return;
        }

        GameObject go = new GameObject("EventBus");
        go.AddComponent<EventBus>();
        DontDestroyOnLoad(go);
    }

    private static void CreateInputManager()
    {
        InputManager existing = FindFirstObjectByType<InputManager>();
        if (existing != null)
        {
            DontDestroyOnLoad(existing.gameObject);
            return;
        }

        GameObject go = new GameObject("InputManager");
        InputManager inputManager = go.AddComponent<InputManager>();

        inputManager.inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");

        if (inputManager.inputActions == null)
        {
            Debug.LogError("InputManager: Не удалось загрузить InputSystem_Actions! " +
                "Убедитесь, что файл InputSystem_Actions.inputactions лежит в папке Assets/Resources/");
        }

        DontDestroyOnLoad(go);
    }
}
