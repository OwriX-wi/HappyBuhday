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
    private float resetDelay = 0.5f; // Задержка перед сбросом
    private float resetTimer = 0f;

    void Start()
    {
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
        {
            return;
        }

        if (IsPlayerLookingAtCandle())
        {
            if (InputManager.Instance.IsInteractPressed())
            {
                Debug.Log("Button is being held.");
                holdTime += Time.deltaTime;
                resetTimer = 0f; // Сбрасываем таймер сброса
                if (holdTime >= 3f)
                {
                    StopCandleVFX();
                }
            }
            else
            {
                Debug.Log("Button is not being held.");
                resetTimer += Time.deltaTime;
                if (resetTimer >= resetDelay)
                {
                    ResetHoldTime();
                }
            }
        }
        else
        {
            ResetHoldTime();
        }
    }

    private void StopCandleVFX()
    {
        if (candleParticleSystem != null)
        {
            Debug.Log("Stopping ParticleSystem...");
            candleParticleSystem.Stop();
            candleVFX.SetActive(false);
            Debug.Log("CandleVFXController: ParticleSystem stopped and GameObject deactivated.");
        }
        else
        {
            Debug.LogError("CandleVFXController: ParticleSystem is null!");
        }

        isVFXActive = false;
        Debug.Log("CandleVFXController: Candle VFX is now inactive.");

        // Добавьте эту строку:
        CandleManager.Instance?.OnCandleExtinguished();
    }

    private void ResetHoldTime()
    {
        if (holdTime > 0f)
        {
            Debug.Log("Hold time reset.");
            holdTime = 0f;
        }
    }

    private bool IsPlayerLookingAtCandle()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Candle");

        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);

        bool isLooking = Physics.Raycast(ray, out hit, interactDistance, layerMask) &&
                         hit.collider != null &&
                         hit.collider.gameObject == this.gameObject;

        return isLooking;
    }
}