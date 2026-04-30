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
            Debug.Log("Candle VFX is inactive. Update skipped.");
            return;
        }

        if (IsPlayerLookingAtCandle())
        {
            if (InputManager.Instance.IsInteractPressed())
            {
                holdTime += Time.deltaTime;
                Debug.Log($"Hold time: {holdTime}"); // Лог текущего времени удержания
                if (holdTime >= 3f)
                {
                    Debug.Log("Hold time reached 3 seconds. Calling StopCandleVFX().");
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
    }

    private void StopCandleVFX()
    {
        if (candleParticleSystem != null)
        {
            Debug.Log("Stopping ParticleSystem...");
            candleParticleSystem.Stop();
            candleVFX.SetActive(false); // Полностью отключаем объект
            Debug.Log("CandleVFXController: ParticleSystem stopped and GameObject deactivated.");
        }
        else
        {
            Debug.LogError("CandleVFXController: ParticleSystem is null!");
        }

        isVFXActive = false;
        Debug.Log("CandleVFXController: Candle VFX is now inactive.");
    }

    private void ResetHoldTime()
    {
        holdTime = 0f;
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