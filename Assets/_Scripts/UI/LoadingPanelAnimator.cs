using System.Collections;
using UnityEngine;

public class LoadingPanelAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform hourglassTransform; // Ссылка на RectTransform песочных часов
    [SerializeField] private float rotationSpeed = 200f; // Скорость вращения
    [SerializeField] private float pauseDuration = 0.5f; // Длительность паузы после поворота на 180 градусов

    private bool isAnimating = true;

    void Start()
    {
        if (hourglassTransform == null)
        {
            Debug.LogError("Hourglass RectTransform is not assigned!");
            return;
        }

        StartCoroutine(AnimateHourglass());
    }

    private IEnumerator AnimateHourglass()
    {
        while (isAnimating)
        {
            // Поворот на 180 градусов
            float targetAngle = hourglassTransform.eulerAngles.z + 180f;
            while (Mathf.Abs(hourglassTransform.eulerAngles.z - targetAngle) > 0.1f)
            {
                float step = rotationSpeed * Time.deltaTime;
                hourglassTransform.rotation = Quaternion.RotateTowards(
                    hourglassTransform.rotation,
                    Quaternion.Euler(0, 0, targetAngle),
                    step
                );
                yield return null;
            }

            // Пауза после поворота
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    public void StopAnimation()
    {
        isAnimating = false;
    }

    public void StartAnimation()
    {
        if (!isAnimating)
        {
            isAnimating = true;
            StartCoroutine(AnimateHourglass());
        }
    }
}