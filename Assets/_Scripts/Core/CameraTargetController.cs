using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    // ДОБАВИЛИ: Ссылка для быстрого доступа из меню настроек
    public static CameraTargetController Instance { get; private set; }

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("State")]
    [SerializeField] private float currentYaw = 0f;    // Y
    [SerializeField] private float currentPitch = 20f; // X

    private void Awake()
    {
        // Инициализируем синглтон камеры
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Инициализация углов камеры
        Vector3 euler = transform.localRotation.eulerAngles;
        currentYaw = euler.y;
        currentPitch = NormalizeAngle(euler.x);
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
        transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }

    private void Start()
    {
        // При старте уровня подгружаем чувствительность, которую сохранил слайдер
        // Если игра запускается впервые, выставится значение по умолчанию (0.15f)
        mouseSensitivity = PlayerPrefs.GetFloat("SavedMouseSensitivity", 0.15f);
    }
    private void Update()
    {
        if (InputManager.Instance == null)
            return;

        Vector2 lookInput = InputManager.Instance.LookInput;
        if (lookInput == Vector2.zero)
            return;

        // ВРЕМЕННЫЙ ТЕСТ: Выводим в консоль реальную чувствительность скрипта камеры
        Debug.Log($"[CAMERA SCRIPT] Текущая чувствительность в камере: {mouseSensitivity}");

        // Рассчитываем вращение камеры
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        currentYaw += mouseX;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);

        transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }
    public void SetMouseSensitivity(float sensitivity)
    {
        // Расширил диапазон до 0.01 - 2.0, так как для New Input System 10f — это слишком быстро
        mouseSensitivity = Mathf.Clamp(sensitivity, 0.01f, 2.0f);
        
        // Сохраняем значение в память
        PlayerPrefs.SetFloat("SavedMouseSensitivity", mouseSensitivity);
    }

    public float GetMouseSensitivity() => mouseSensitivity;

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}