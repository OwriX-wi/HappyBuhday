using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

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

    public void Load(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    public void LoadAsync(string sceneNames)
    {
        StartCoroutine(LoadSceneAsyncCoroutine(sceneNames));
    }

    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            Debug.Log($"Loading progress: {operation.progress}");
            yield return null;
        }
        Debug.Log($"Scene {sceneName} loaded successfully.");

        // yield return null - 1 кадр
        // yield return new WaitForSeconds(2f); - ждать 2 секунды
        // StartCoroutine(AnotherCoroutine()); - запустить другую корутину и ждать её окончания
        // yield break; - завершить корутину досрочно
        // yield return operation; - ждать завершения асинхронной операции
    }
}