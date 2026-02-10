using UnityEngine;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button buttonResume;
    [SerializeField] private Button buttonMainMenu;

    private void OnEnable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnGamePaused += ShowPausePanel;
            EventBus.Instance.OnGameResumed += HidePausePanel;
        }
    }

    private void OnDisable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnGamePaused -= ShowPausePanel;
            EventBus.Instance.OnGameResumed -= HidePausePanel;
        }
    }

    void Start()
    {
        if (buttonResume != null)
            buttonResume.onClick.AddListener(OnResumeClicked);
        if (buttonMainMenu != null)
            buttonMainMenu.onClick.AddListener(OnMainMenuClicked);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager instance is null. Cannot toggle pause.");
            return;
        }


        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            GameManager.Instance.Pause();
        }
        else if (GameManager.Instance.CurrentState == GameState.Paused)
        {
            GameManager.Instance.Resume();
        }
    }

    private void HidePausePanel()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void ShowPausePanel()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }


    private void OnResumeClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.Resume();
    }

    private void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMenu();
    }
}
