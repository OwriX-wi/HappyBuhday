using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine; // ОБЯЗАТЕЛЬНО для работы с новым Cinemachine

public class PauseSettingsController : MonoBehaviour
{
    [Header("Слайдеры (UI)")]
    public Slider sensitivitySlider;

    [Header("Настройки диапазона Cinemachine")]
    // Так как в Cinemachine базовый Gain для LookX равен 12, настроим адекватный диапазон
    public float minGainX = 2f;
    public float maxGainX = 30f;
    public float sensitivityCurve = 1.5f; // Небольшой изгиб для плавности

    // Ссылки на компонент Cinemachine
    private CinemachineInputAxisController axisController;

    private void Start()
    {
        FindCinemachineController();

        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            sensitivitySlider.minValue = 0f;
            sensitivitySlider.maxValue = 1f;

            // Загружаем сохраненное положение ползунка (по умолчанию 0.4f)
            sensitivitySlider.value = PlayerPrefs.GetFloat("SavedMouseSliderValue", 0.4f);
        }
    }

    private void FindCinemachineController()
    {
        // Ищем контроллер ввода Cinemachine на сцене
        axisController = FindFirstObjectByType<CinemachineInputAxisController>();

        if (axisController == null)
        {
            Debug.LogWarning("SettingsMenu: Компонент CinemachineInputAxisController не найден на сцене!");
        }
    }

    private void OnSensitivityChanged(float sliderValue)
    {
        // Применяем кривую скейлинга
        float exponentialValue = Mathf.Pow(sliderValue, sensitivityCurve);

        // Считаем итоговый Gain для горизонтальной оси (Look X)
        float finalGainX = Mathf.Lerp(minGainX, maxGainX, exponentialValue);

        // Считаем Gain для вертикальной оси (Look Y). В Cinemachine она обычно инвертирована (с минусом)
        // На вашем скриншоте X = 12, а Y = -10. Сохраняем это соотношение:
        float finalGainY = finalGainX * (-10f / 12f);

        // Передаем значения напрямую в Cinemachine
        if (axisController == null) FindCinemachineController();

        if (axisController != null)
        {
            // Защита: проверяем, что в массиве Driven Axes есть наши оси
            if (axisController.Controllers.Count >= 2)
            {
                // ИСПРАВЛЕНО: добавляем .Input перед .Gain для Cinemachine v3
                axisController.Controllers[0].Input.Gain = finalGainX; // Для Look X
                axisController.Controllers[1].Input.Gain = finalGainY; // Для Look Y
            }
        }

        // Сохраняем положение слайдера
        PlayerPrefs.SetFloat("SavedMouseSliderValue", sliderValue);
    }

    private void OnDestroy()
    {
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
    }
}