using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Assets._Scripts.Enemy
{
    public class DetectionComponent : MonoBehaviour
    {
        [Header("Sound Detection")]
        [SerializeField] private float soundDetectionRadius = 15f;
        public float SoundDetectionRadius => soundDetectionRadius;

        [Tooltip("Порог в секундах, ниже — короткий звук (только посмотреть), выше — длинный (бежать)")]
        public float shortSoundThreshold = 2f;
        [Tooltip("Как долго враг будет смотреть на источник короткого звука")]
        public float lookDurationShort = 1f;
        [Tooltip("Множитель скорости проигрывания анимации Detect для длинного звука")]
        public float detectFastMultiplier = 1.6f;
        [Tooltip("Скорость поворота при поворачивании к источнику")]
        public float lookRotationSpeed = 6f;
        [Tooltip("Как долго будет преследовать позицию звука (сек)")]
        public float soundChaseDuration = 5f;
        [Tooltip("Расстояние до позиции звука, при достижении прекратить преследование")]
        public float soundStopDistance = 1.2f;
        [Tooltip("Если игрок находится в этом радиусе от позиции звука, враг сразу выявляет игрока")]
        public float playerCatchRadius = 2f;

        [Header("Sight Detection")]
        [SerializeField] private float sightRadius = 5f;

        [Header("Patrol Settings")]
        [Tooltip("Точки патрулирования")]
        [SerializeField] private List<Transform> patrolPoints;
        [Tooltip("Скорость патрулирования")]
        [SerializeField] private float patrolSpeed = 2f;
        [Tooltip("Время ожидания на точке патрулирования")]
        [SerializeField] private float waitTimeAtPoint = 2f;

        public Animator animator;
        public Rigidbody rb;
        public Transform player;
        public GameObject target;

        public bool SeePlayer { get; private set; }
        public bool MoveToSoundPosition { get; private set; }
        public Vector3 SoundSourcePosition { get; private set; }
        public bool IsPatrolling => isPatrolling && patrolPoints.Count > 0 && !SeePlayer && !MoveToSoundPosition;

        private Collider[] hitColliders = new Collider[10];
        private RaycastHit hit;

        private Vector3 lastKnownPlayerPosition;
        private bool chasingPlayerDirectly = false;

        private int currentPatrolIndex = 0;
        private bool isPatrolling = true;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (player == null)
            {
                var pgo = GameObject.FindGameObjectWithTag("Player");
                if (pgo != null) player = pgo.transform;
            }

            if (EventBus.Instance != null)
            {
                EventBus.Instance.OnPlayerMadeSound += ReceiveSound;
            }
        }

        private void OnDestroy()
        {
            if (EventBus.Instance != null)
            {
                EventBus.Instance.OnPlayerMadeSound -= ReceiveSound;
            }
        }

        private void Start()
        {
            if (patrolPoints.Count > 0)
            {
                StartCoroutine(Patrol());
            }
        }

        private IEnumerator Patrol()
        {
            Debug.Log("Начало патрулирования");
            while (isPatrolling)
            {
                Debug.Log("Патрулирование активно");
    
            if (SeePlayer || MoveToSoundPosition)
                {
                    yield return null; // Прекращаем патрулирование, если враг обнаружил игрока или звук
                    continue;
                }

                Transform targetPoint = patrolPoints[currentPatrolIndex];
                Vector3 direction = (targetPoint.position - transform.position).normalized;
                float distance = Vector3.Distance(transform.position, targetPoint.position);

                // Двигаемся к текущей точке патрулирования
                while (distance > 0.5f && !SeePlayer && !MoveToSoundPosition)
                {
                    direction = (targetPoint.position - transform.position).normalized;
                    distance = Vector3.Distance(transform.position, targetPoint.position);

                    rb.MovePosition(transform.position + direction * patrolSpeed * Time.deltaTime);

                    // Поворачиваем врага в сторону движения
                    if (direction.sqrMagnitude > 0.01f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, lookRotationSpeed * Time.deltaTime));
                    }

                    if (animator != null)
                    {
                        animator.SetFloat("Speed_f", patrolSpeed);
                    }

                    yield return null;
                }

                // Ожидание на точке
                if (!SeePlayer && !MoveToSoundPosition)
                {
                    if (animator != null)
                    {
                        animator.SetFloat("Speed_f", 0f);
                    }
                    yield return new WaitForSeconds(waitTimeAtPoint);
                }

                // Переход к следующей точке
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
            }
        }

        public void ReceiveSound(Vector3 pos, float duration)
        {
                if (Vector3.Distance(transform.position, pos) > soundDetectionRadius)
                    return; // Игнорируем звук, если он слишком далеко
    
                if (Vector3.Distance(pos, player.position) <= playerCatchRadius)
                {
                    // Если игрок находится в радиусе "поймать игрока", сразу выявляем его
                    target = player.gameObject;
                    SeePlayer = true;
                    if (animator != null)
                    {
                        animator.SetTrigger("Detect");
                        animator.SetBool("Detected", true);
                    }
                    return;
                }
    
                SoundSourcePosition = pos;
                MoveToSoundPosition = true;

            lastKnownPlayerPosition = pos;

            if (animator != null)
            {
                float speedMul = duration >= shortSoundThreshold ? detectFastMultiplier : 1f;
                animator.SetFloat("DetectSpeed", speedMul);
                animator.SetTrigger("Detect");
            }

            if (duration >= shortSoundThreshold)
            {
                chasingPlayerDirectly = true;
                StopAllCoroutines();
                StartCoroutine(HandleSoundChase(player.position));
            }
            else
            {
                chasingPlayerDirectly = false;
                StopAllCoroutines();
                StartCoroutine(HandleSoundChase(pos));
            }
        }

        private string HandleSoundChase(Vector3 pos)
        {
            throw new NotImplementedException();
        }

        public void SightDetection()
        {
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, sightRadius, hitColliders);
            for (int i = 0; i < hitCount; i++)
            {
                var hitCollider = hitColliders[i];
                if (hitCollider.CompareTag("Player"))
                {
                    var playerRb = hitCollider.GetComponent<Rigidbody>();
                    if (playerRb != null && playerRb.linearVelocity.magnitude > 10f)
                    {
                        Debug.Log("Игрок обнаружен в радиусе обзора.");
                        target = hitCollider.gameObject;
                        SeePlayer = true;
                        if (animator != null)
                        {
                            animator.SetTrigger("Detect");
                            animator.SetBool("Detected", true);
                        }
                        break;
                    }
                }
            }
        }
        public Vector3 GetPatrolDirection()
        {
            if (!IsPatrolling) return Vector3.zero;
            Transform targetPoint = patrolPoints[currentPatrolIndex];
            Vector3 direction = (targetPoint.position - transform.position);
            direction.y = 0f;
            return direction.normalized;
        }
        public float GetPatrolSpeed() => patrolSpeed;
        public void UpdatePatrolPointIfNeeded()
        {
            if (!IsPatrolling) return;
            Transform targetPoint = patrolPoints[currentPatrolIndex];
            float distance = Vector3.Distance(transform.position, targetPoint.position);
            if (distance <= 0.5f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
            }
        }
    }
}
