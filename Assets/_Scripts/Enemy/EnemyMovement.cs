using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform Target;

    [Header("Настройки")]
    public float UpdateSpeed = 0.1f;
    public float HearingDistance = 20f;

    private NavMeshAgent Agent;
    private bool _isPlayerSprinting = false;
    private Coroutine _followCoroutine;

    // Ссылка на действие (Action). 
    // В идеале оно должно быть в скрипте игрока, например: PlayerInput.OnSprintChanged
    public static Action<bool> OnSprintChanged;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        // Подписываемся на событие
        OnSprintChanged += HandleSprint;

        // Запускаем корутину
        _followCoroutine = StartCoroutine(FollowTarget());
    }

    private void OnDisable()
    {
        // Обязательно отписываемся при уничтожении/деактивации объекта
        OnSprintChanged -= HandleSprint;

        if (_followCoroutine != null) StopCoroutine(_followCoroutine);
    }

    private void HandleSprint(bool isSprinting)
    {
        _isPlayerSprinting = isSprinting;
    }

    private IEnumerator FollowTarget()
    {
        WaitForSeconds wait = new WaitForSeconds(UpdateSpeed);

        while (enabled)
        {
            // Проверяем: бежит ли игрок И слышим ли мы его по дистанции
            if (_isPlayerSprinting && Vector3.Distance(transform.position, Target.position) <= HearingDistance)
            {
                Agent.SetDestination(Target.position);
            }
            else
            {
                // Если звук пропал, враг может остановиться, дойдя до последней точки
                if (Agent.hasPath && Agent.remainingDistance < 0.5f)
                {
                    Agent.ResetPath();
                }
            }

            yield return wait;
        }
    }
}