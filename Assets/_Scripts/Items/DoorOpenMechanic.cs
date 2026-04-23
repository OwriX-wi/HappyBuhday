using UnityEngine;

public class DoorOpenMechanic : MonoBehaviour
{
    public float openAngle = 110f;
    public float openSpeed = 2.5f;
    private bool isPlayerNear = false;
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    [SerializeField] private GameObject doorPivot;
    private Transform playerTransform;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation; // По умолчанию
    }

    void Update()
    {
        if (isPlayerNear && InputManager.Instance.IsInteractPressed())
        {
            isOpen = !isOpen;

            if (isOpen)
            {
                // Определяем сторону открытия
                if (playerTransform != null)
                {
                    Vector3 toPlayer = playerTransform.position - transform.position;
                    float side = Vector3.Dot(toPlayer, transform.right);

                    float angle = (side > 0) ? openAngle : -openAngle;
                    openRotation = Quaternion.AngleAxis(angle, Vector3.up) * closedRotation;
                }
            }
        }

        // Плавное вращение двери
        RotateDoor(isOpen ? openRotation : closedRotation);
    }

    private void RotateDoor(Quaternion targetRotation)
    {
        // Плавное вращение двери к целевому положению
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);

        // Убедимся, что дверь точно возвращается в изначальное положение
        if (!isOpen && Quaternion.Angle(transform.rotation, closedRotation) < 0.01f)
        {
            transform.rotation = closedRotation; // Устанавливаем точное начальное положение
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerTransform = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerTransform = null;
        }
    }
}
