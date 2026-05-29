using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyNavMeshIntegration : MonoBehaviour
{
    public NavMeshAgent agent;
    public Rigidbody rb;
    public Transform target;
    public Animator animator;

    [Tooltip("Дистанция, на которой враг начинает анимацию атаки")]
    public float attackRange = 1.5f;
    [Tooltip("Время между повторными атаками (сек)")]
    public float attackCooldown = 1.2f;

    private bool isAttacking;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();

        if (agent == null)
        {
            Debug.LogWarning("NavMeshAgent не найден на объекте.");
            enabled = false;
            return;
        }

        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.nextPosition = transform.position;

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void Update()
    {
        if (agent.isOnNavMesh && target != null)
        {
            agent.SetDestination(target.position);
        }

        // Обновляем параметры аниматора (Speed_f и Detected)
        if (animator != null && rb != null)
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            animator.SetFloat("Speed_f", flatVel.magnitude);

            bool detected = target != null;
            animator.SetBool("Detected", detected);
        }

        // Проверяем возможность атаки (по дистанции)
        if (!isAttacking && target != null)
        {
            float dist = Vector3.Distance(rb != null ? rb.position : transform.position, target.position);
            if (dist <= attackRange)
            {
                if (animator != null) animator.SetTrigger("Attack");
                StartCoroutine(AttackCooldown());
            }
        }
    }

    void FixedUpdate()
    {
        if (agent == null || rb == null) return;

        Vector3 nextPos = agent.nextPosition;
        Vector3 newPos = Vector3.MoveTowards(rb.position, nextPos, agent.speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        Vector3 desiredVel = agent.desiredVelocity;
        Vector3 flatVel = new Vector3(desiredVel.x, 0f, desiredVel.z);
        if (flatVel.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatVel);
            Quaternion newRot = Quaternion.RotateTowards(rb.rotation, targetRot, agent.angularSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRot);
        }

        agent.nextPosition = rb.position;
    }

    private IEnumerator AttackCooldown()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }
}