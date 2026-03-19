using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("State")]
    [SerializeField] private float currentYaw = 0f;    // Y
    [SerializeField] private float currentPitch = 20f; // X

    private void Awake()
    {
        // Инициализация углов камеры
        Vector3 euler = transform.localRotation.eulerAngles;
        currentYaw = euler.y;
        currentPitch = NormalizeAngle(euler.x);
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
        transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

        // Блокировка курсора для управления камерой
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Проверяем наличие InputManager
        if (InputManager.Instance == null)
            return;

        // Получаем ввод мыши
        Vector2 lookInput = InputManager.Instance.GetLookInput();
        if (lookInput == Vector2.zero)
            return;

        // Рассчитываем вращение камеры
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        currentYaw += mouseX;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);

        // Применяем вращение к камере
        transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f);
    }

    public float GetMouseSensitivity() => mouseSensitivity;

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}
