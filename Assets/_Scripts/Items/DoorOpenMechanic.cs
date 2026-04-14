using UnityEngine;
using UnityEngine.Animations;
using System.Collections;

public class DoorOpenMechanic : MonoBehaviour
{
    public float openAngle = 110f; // Угол открытия двери
    public float openSpeed = 2.5f;  // Скорость открытия
    private bool isPlayerNear = false;
    private bool isOpen = false;
    private Quaternion closedRotation; // Закрытое положение
    private Quaternion openRotation;   // Открытое положение
    [SerializeField] private GameObject doorPivot; // Точка вращения двери
    private Coroutine autoCloseCoroutine; // Ссылка на корутину для закрытия двери

    void Start()
    {
        // Сохраняем начальное и конечное вращение относительно doorPivot
        closedRotation = transform.rotation;
        openRotation = Quaternion.AngleAxis(openAngle, Vector3.up) * closedRotation;
    }

    void Update()
    {
        // Проверяем, находится ли игрок рядом и нажата ли клавиша "E"
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen;

            if (isOpen == true)
            {
                // Если дверь открывается, запускаем корутину для автоматического закрытия
                if (autoCloseCoroutine != null)
                {
                    StopCoroutine(autoCloseCoroutine); // Останавливаем предыдущую корутину, если она запущена
                }
                autoCloseCoroutine = StartCoroutine(AutoCloseDoor());
            }
            else
            {
                // Если дверь закрывается вручную, останавливаем корутину
                if (autoCloseCoroutine != null)
                {
                    StopCoroutine(autoCloseCoroutine);
                    autoCloseCoroutine = null;
                }
            }
        }

        // Плавное вращение двери вокруг doorPivot
        RotateDoor(isOpen ? openRotation : closedRotation);
    }

    private void RotateDoor(Quaternion targetRotation)
    {
        // Рассчитываем вращение относительно doorPivot
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
    }

    private IEnumerator AutoCloseDoor()
    {
        // Ждём 7 секунд
        yield return new WaitForSeconds(5f);

        // Закрываем дверь
        isOpen = false;
        autoCloseCoroutine = null; // Сбрасываем ссылку на корутину
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerNear = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerNear = false;
    }
}
