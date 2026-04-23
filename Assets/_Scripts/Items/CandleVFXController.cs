using UnityEngine;

//сделать через инпут и рейкаст, который определяет находится ли игрок рядом со свечой, и если он держит кнопку 3 секунды, то VFX гаснет. И при этом не должно быть никаких багов с повторным включением и выключением VFX.

public class CandleVFXController : MonoBehaviour
{
    [SerializeField] private GameObject candleVFX; // Ссылка на VFX свечи
    [SerializeField] private float interactDistance = 3f; // Дистанция взаимодействия
    private float holdTime = 0f;
    private bool isVFXActive = true;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (!isVFXActive)
            return;

        if (IsPlayerLookingAtCandle())
        {
            Debug.Log("Player is looking at the candle.");
            if (InputManager.Instance.IsInteractPressed())
            {
                holdTime += Time.deltaTime;
                if (holdTime >= 3f)
                {
                    var ps = candleVFX.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        ps.Stop();
                        Debug.Log("ParticleSystem stopped.");
                    }
                    
                    isVFXActive = false;
                    Debug.Log("Candle VFX is now inactive.");
                }
            }
            else
            {
                if (holdTime > 0)
                    Debug.Log("Hold time reset.");
                holdTime = 0f;
            }
        }
        else
        {
            if (holdTime > 0)
                Debug.Log("Player is no longer looking at the candle. Hold time reset.");
            holdTime = 0f;
        }
    }

    private bool IsPlayerLookingAtCandle()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider != null && hit.collider.gameObject == this.gameObject)
            {
                return true;
            }
        }
        return false;
    }
}