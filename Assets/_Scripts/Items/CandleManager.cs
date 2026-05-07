using UnityEngine;

public class CandleManager : MonoBehaviour
{
    public static CandleManager Instance { get; private set; }

    [SerializeField] private int totalCandles = 5;
    [SerializeField] private float minAmbientIntensity = 0.2f;
    [SerializeField] private float minSkyboxExposure = 0.2f;
    [SerializeField] private float initialAmbientIntensity = 0.5f;
    [SerializeField] private float initialSkyboxExposure = 1.0f;


    private int extinguishedCandles = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Сохраняем изначальные значения
        initialAmbientIntensity = RenderSettings.ambientIntensity;
        if (RenderSettings.skybox.HasProperty("_Exposure"))
            initialSkyboxExposure = RenderSettings.skybox.GetFloat("_Exposure");
        else
            initialSkyboxExposure = 1.0f;

        // Выводим значения в консоль
        Debug.Log($"Initial Ambient Intensity: {initialAmbientIntensity}");
        Debug.Log($"Initial Skybox Exposure: {initialSkyboxExposure}");
    }

    public void OnCandleExtinguished()
    {
        extinguishedCandles++;
        UpdateEnvironmentDarkness();
    }

    private void UpdateEnvironmentDarkness()
    {
        float t = Mathf.Clamp01((float)extinguishedCandles / totalCandles);

        // Меняем ambient intensity
        RenderSettings.ambientIntensity = Mathf.Lerp(initialAmbientIntensity, minAmbientIntensity, t);

        // Меняем skybox exposure, если возможно
        if (RenderSettings.skybox.HasProperty("_Exposure"))
        {
            float newExposure = Mathf.Lerp(initialSkyboxExposure, minSkyboxExposure, t);
            RenderSettings.skybox.SetFloat("_Exposure", newExposure);
        }
    }
}