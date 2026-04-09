using UnityEngine;

public class DoorController : MonoBehaviour
{
    private HingeJoint hinge;
    private bool isOpen = false;

    void Start()
    {
        hinge = GetComponent<HingeJoint>();
        // Убедимся, что лимиты включены, чтобы дверь не крутилась на 360 градусов
        hinge.useLimits = true;
    }

    void Update()
    {
        // Проверка нажатия клавиши E (можно заменить на ваш метод взаимодействия)
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleDoor();
        }
    }

    void ToggleDoor()
    {
        isOpen = !isOpen;

        // Используем пружину для плавного перемещения к углу
        hinge.useSpring = true;
        JointSpring spr = hinge.spring;

        // Устанавливаем целевой угол: 90 градусов (открыто) или 0 (закрыто)
        spr.targetPosition = isOpen ? 90f : 0f;

        // Сила и сопротивление пружины (можно настроить в инспекторе)
        spr.spring = 10f;
        spr.damper = 3f;

        hinge.spring = spr;
    }
}
