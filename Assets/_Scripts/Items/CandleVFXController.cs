using UnityEngine;

//сделать через инпут и рейкаст, который определяет находится ли игрок рядом со свечой, и если он держит кнопку 3 секунды, то VFX гаснет. И при этом не должно быть никаких багов с повторным включением и выключением VFX.

public class CandleVFXController : MonoBehaviour
{
    [SerializeField] private GameObject candleVFX;
    [SerializeField] private float interactDistance = 3f;
    private float holdTime = 0f;
    private bool isVFXActive = true;
    private Camera mainCamera;
    private ParticleSystem candleParticleSystem;
    private bool isInteractPressed = false;

    void Start()
    {
        // Подписка на событие OnInteractPressed
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnInteractPressed += HandleInteractPressed;
        }
        else
        {
            Debug.LogError("CandleVFXController: InputManager instance not found!");
        }

        mainCamera = Camera.main;
        candleParticleSystem = candleVFX.GetComponent<ParticleSystem>();

        if (candleParticleSystem == null)
        {
            Debug.LogError("CandleVFXController: ParticleSystem not found on candleVFX!");
        }
    }

    void Update()
    {
        if (!isVFXActive)
            return;

        if (IsPlayerLookingAtCandle())
        {
            if (isInteractPressed) // Используем флаг, установленный событием
            {
                Debug.Log("Interact button pressed.");
                holdTime += Time.deltaTime;
                if (holdTime >= 3f)
                {
                    StopCandleVFX();
                }
            }
            else
            {
                ResetHoldTime();
            }
        }
        else
        {
            ResetHoldTime();
        }

        // Сбрасываем флаг после обработки
        isInteractPressed = false;
    }

    private void StopCandleVFX()
    {
        if (candleParticleSystem != null)
        {
            candleParticleSystem.Stop();
            Debug.Log("CandleVFXController: ParticleSystem stopped.");
        }

        isVFXActive = false;
        Debug.Log("CandleVFXController: Candle VFX is now inactive.");
    }

    private void ResetHoldTime()
    {
        if (holdTime > 0)
        {
            Debug.Log("CandleVFXController: Hold time reset.");
        }
        holdTime = Mathf.Max(holdTime - Time.deltaTime, 0f); // Плавное уменьшение времени
    }

    private bool IsPlayerLookingAtCandle()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Candle"); // Убедитесь, что свеча на слое "Candle"

        // Визуализация луча
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);

        bool isLooking = Physics.Raycast(ray, out hit, interactDistance, layerMask) &&
                         hit.collider != null &&
                         hit.collider.gameObject == this.gameObject;

        if (isLooking)
        {
            Debug.Log("CandleVFXController: Player is looking at the candle.");
        }
        else
        {
            Debug.Log("CandleVFXController: Player is NOT looking at the candle.");
        }

        return isLooking;
    }

    private void HandleInteractPressed()
    {
        isInteractPressed = true;
    }

    private void OnDestroy()
    {
        // Отписка от события OnInteractPressed
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnInteractPressed -= HandleInteractPressed;
        }
    }
}