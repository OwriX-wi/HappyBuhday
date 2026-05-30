using UnityEngine;
using UnityEngine.Rendering; // Нужно для работы с Volume
using UnityEngine.Rendering.Universal; // Нужно, если используете URP

public class CandleManager : MonoBehaviour
{
    public static CandleManager Instance { get; private set; }

    [Header("Настройки свечей")]
    [SerializeField] private int totalCandles = 5;
    private int extinguishedCandles = 0;

    [Header("Настройки затемнения через Post-Processing")]
    [SerializeField] private Volume postProcessVolume;

    [Tooltip("Экспозиция при зажженных свечах (нормальное состояние)")]
    [SerializeField] private float initialExposure = 0f;

    [Tooltip("На сколько опустится экспозиция, когда все свечи потухнут (минус делает темнее)")]
    [SerializeField] private float minExposure = -3f;

    private ColorAdjustments colorAdjustments;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Ищем компонент Color Adjustments в профиле Volume
        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out colorAdjustments))
        {
            // Принудительно включаем управление экспозицией
            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.postExposure.value = initialExposure;
        }
        else
        {
            Debug.LogError("CandleManager: Не найден Volume или эффект Color Adjustments в его профиле!");
        }
    }

    public void OnCandleExtinguished()
    {
        extinguishedCandles++;
        UpdateEnvironmentDarkness();
    }
    private void UpdateEnvironmentDarkness()
    {
        float t = Mathf.Clamp01((float)extinguishedCandles / totalCandles);

        if (colorAdjustments != null)
        {
            // ПРИНУДИТЕЛЬНО: Говорим Unity, что этот параметр сейчас ОГЛАВЛЕН скриптом
            colorAdjustments.postExposure.overrideState = true;

            // Плавно понижаем экспозицию
            colorAdjustments.postExposure.value = Mathf.Lerp(initialExposure, minExposure, t);

            Debug.Log($"Candles: {extinguishedCandles}/{totalCandles}. Real Volume Value: {colorAdjustments.postExposure.value}");
        }
    }
}