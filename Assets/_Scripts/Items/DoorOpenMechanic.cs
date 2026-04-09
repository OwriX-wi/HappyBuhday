using UnityEngine;

public class DoorOpenMechanic : MonoBehaviour
{
    public float openAngle = 90f; // угол открыти€ двери
    public float openSpeed = 2f;  // скорость открыти€
    private bool isPlayerNear = false;
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        // —охран€ем начальное и конечное вращение
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + Vector3.up * openAngle);
    }

    void Update()
    {
        // ѕровер€ем нажатие клавиши "E" и переключаем состо€ние двери
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen;
        }

        // ѕлавное вращение двери
        if (isOpen)
            transform.rotation = Quaternion.Slerp(transform.rotation, openRotation, Time.deltaTime * openSpeed);
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, closedRotation, Time.deltaTime * openSpeed);
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
