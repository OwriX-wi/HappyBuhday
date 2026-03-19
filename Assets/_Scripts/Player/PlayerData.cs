using UnityEngine;

[CreateAssetMenu(fileName = "New Player Data", menuName = "Game Data/Player Data", order = 0)]
/// <summary>
/// ScriptableObject с базовыми параметрами игрока.
/// ’ранит стартовые значени€ здоровь€, маны и настроек движени€,
/// которые затем читают PlayerStats и PlayerController.
/// </summary>
public class PlayerData : ScriptableObject
{
    [Header("ќсновные характеристики")]
    [Min(1f)]
    [Tooltip("ћаксимальное здоровье игрока по умолчанию.")]
    public float maxHealth = 100f;

    [Min(0f)]
    [Tooltip("ћаксимальна€ мана / энерги€ по умолчанию. ћожет быть 0, если мана не используетс€.")]
    public float maxMana = 0f;

    [Header("ƒвижение")]
    [Min(0f)]
    [Tooltip("Ѕазова€ скорость движени€, которую использует PlayerController.")]
    public float moveSpeed = 5f;

    [Min(0f)]
    [Tooltip("Ѕазова€ сила прыжка, вли€ет на начальную вертикальную скорость.")]
    public float jumpForce = 5f;

    [Header("ƒополнительные параметры движени€")]
    [Min(0f)]
    [Tooltip("”скорение при начале движени€ (может использоватьс€ в более сложных контроллерах).")]
    public float acceleration = 10f;

    [Min(0f)]
    [Tooltip("—корость поворота персонажа (градусы в секунду).")]
    public float rotationSpeed = 720f;
}