using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Game Data/Enemy Data", order = 1)]

public class EnemyData : ScriptableObject
{
    [Header("Движение")]
    [Min(0f)]
    [Tooltip("Базовая скорость движения, которую использует PlayerController.")]
    public float moveSpeed = 5f;

    [Min(0f)]
    [Tooltip("Базовая сила прыжка, влияет на начальную вертикальную скорость.")]
    public float jumpForce = 5f;

    [Header("Дополнительные параметры движения")]
    [Min(0f)]
    [Tooltip("Ускорение при начале движения (может использоваться в более сложных контроллерах).")]
    public float acceleration = 10f;

    [Min(0f)]
    [Tooltip("Скорость поворота персонажа (градусы в секунду).")]
    public float rotationSpeed = 720f;
}
