using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public float MaxSpeed;
    public float Speed;

    private Collider[] hitColliders;
    private RaycastHit hit;

    public Rigidbody rb;
    public GameObject target;
    public Transform player;

    private bool seePlayer;

    // Радиус обнаружения звуков
    [SerializeField] private float soundDetectionRadius = 15f;

    // Желательная скорость, применяется в FixedUpdate
    private Vector3 desiredVelocity = Vector3.zero;

    // Animator
    public Animator animator;

    // sight radius (настраиваемое поле)
    [SerializeField] private float sightRadius = 10f;

    // --- звук/детект ---
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

    private bool moveToSoundPosition = false;
    private Vector3 soundSourcePosition;

    void Start()
    {
        Speed = MaxSpeed;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogWarning($"EnemyScript на {name}: Rigidbody не найден. Назначьте в инспекторе или добавьте компонент Rigidbody.");
            }
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Авто-присвоение player, если не задано вручную в инспекторе
        if (player == null)
        {
            var pgo = GameObject.FindGameObjectWithTag("Player");
            if (pgo != null)
                player = pgo.transform;
        }

        // Подписываемся на события (включая OnPlayerMadeSound если он есть)
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnPlayerHitFurniture += OnPlayerHitFurniture;
            EventBus.Instance.OnPlayerStartedRunning += OnPlayerStartedRunning;
            // безопасно подписаться, если событие есть
            try
            {
                EventBus.Instance.OnPlayerMadeSound += ReceiveSound;
            }
            catch { /* игнор если старый EventBus без события */ }
        }

        // Диагностика в Start
        Debug.Log($"{name}: Start() — MaxSpeed={MaxSpeed}, Speed={Speed}, sightRadius={sightRadius}");
        if (rb != null)
            Debug.Log($"{name}: Rigidbody found. isKinematic={rb.isKinematic}, interpolation={rb.interpolation}, collisionDetectionMode={rb.collisionDetectionMode}");
        else
            Debug.LogWarning($"{name}: Rigidbody == null");

        if (animator != null)
        {
            Debug.Log($"{name}: Animator assigned. enabled={animator.enabled}, applyRootMotion={animator.applyRootMotion}, cullingMode={animator.cullingMode}");
            Debug.Log($"{name}: runtimeController={(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "null")}");
            var parms = animator.parameters;
            string list = "Animator Parameters:";
            foreach (var p in parms) list += $" [{p.name}:{p.type}]";
            Debug.Log(list);
        }
        else
        {
            Debug.LogWarning($"{name}: Animator ссылка пустая (animator == null). Перетащите компонент Animator в поле скрипта или добавьте Animator на объект.");
        }

        if (MaxSpeed <= 0f)
            Debug.LogWarning($"{name}: MaxSpeed равен 0 — враг не будет двигаться. Установите MaxSpeed > 0 в инспекторе.");
    }

    void OnDestroy()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnPlayerHitFurniture -= OnPlayerHitFurniture;
            EventBus.Instance.OnPlayerStartedRunning -= OnPlayerStartedRunning;
            try { EventBus.Instance.OnPlayerMadeSound -= ReceiveSound; } catch { }
        }
    }

    // Публичный API: получать звук из внешней системы
    public void ReceiveSound(Vector3 pos, float duration)
    {
        Debug.Log($"{name}: ReceiveSound pos={pos} duration={duration}");
        // всегда триггерим Detect-анимацию
        if (animator != null)
        {
            float speedMul = duration >= shortSoundThreshold ? detectFastMultiplier : 1f;
            animator.SetFloat("DetectSpeed", speedMul);
            animator.SetTrigger("Detect");
        }

        if (duration < shortSoundThreshold)
        {
            StopCoroutine("HandleShortLook");
            StartCoroutine(HandleShortLook(pos));
        }
        else
        {
            StopCoroutine("HandleSoundChase");
            StartCoroutine(HandleSoundChase(pos));
        }
    }

    private System.Collections.IEnumerator HandleShortLook(Vector3 pos)
    {
        float t = 0f;
        Vector3 dir = pos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) yield break;
        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        while (t < lookDurationShort)
        {
            if (rb != null)
            {
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, lookRotationSpeed * Time.deltaTime));
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, lookRotationSpeed * Time.deltaTime);
            }
            t += Time.deltaTime;
            yield return null;
        }
    }

    private System.Collections.IEnumerator HandleSoundChase(Vector3 pos)
    {
        soundSourcePosition = pos;
        moveToSoundPosition = true;
        seePlayer = true;
        if (animator != null) animator.SetBool("Detected", true);

        float timer = 0f;
        while (timer < soundChaseDuration)
        {
            // Если игрок находится рядом с позицией звука — сразу переключаемся на игрока
            Transform p = player;
            if (p == null)
            {
                var pgo = GameObject.FindGameObjectWithTag("Player");
                if (pgo != null) p = pgo.transform;
            }

            if (p != null)
            {
                float playerToSound = Vector3.Distance(p.position, soundSourcePosition);
                if (playerToSound <= playerCatchRadius)
                {
                    Debug.Log($"{name}: Player caught near sound position.");
                    target = p.gameObject;
                    moveToSoundPosition = false;
                    seePlayer = true;
                    if (animator != null) animator.SetBool("Detected", true);
                    yield break;
                }
            }

            if (Vector3.Distance(transform.position, soundSourcePosition) <= soundStopDistance) break;

            timer += Time.deltaTime;
            yield return null;
        }

        moveToSoundPosition = false;
        seePlayer = false;
        if (animator != null) animator.SetBool("Detected", false);
    }

    void Update()
    {
        if (rb == null) return;

        if (moveToSoundPosition)
        {
            Vector3 dir = soundSourcePosition - transform.position;
            dir.y = 0f;
            float dist = dir.magnitude;
            if (dist > 0.01f)
            {
                Vector3 direction = dir / dist;
                desiredVelocity = new Vector3(direction.x * Speed, rb.linearVelocity.y, direction.z * Speed);
                Vector3 look = new Vector3(direction.x, 0f, direction.z);
                if (look.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(look);
                    rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, 360f * Time.deltaTime));
                }
            }
            else
            {
                desiredVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }

            if (animator != null)
            {
                Vector3 flatVel = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
                animator.SetFloat("Speed_f", flatVel.magnitude);
            }

            return;
        }

        // обычная логика поиска/движения по зрению
        if (!seePlayer)
        {
            hitColliders = Physics.OverlapSphere(transform.position, sightRadius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    target = hitCollider.gameObject;
                    seePlayer = true;

                    Debug.Log($"{name}: Player found by sight (OverlapSphere).");

                    if (animator != null)
                    {
                        animator.SetTrigger("Detect");
                        animator.SetBool("Detected", true);
                    }

                    break;
                }
            }
        }
        else
        {
            if (target == null)
            {
                seePlayer = false;
                if (animator != null) animator.SetBool("Detected", false);
                return;
            }

            Vector3 dirToTarget = target.transform.position - transform.position;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
            Vector3 rayDir = (target.transform.position + Vector3.up * 0.5f) - rayOrigin;

            if (Physics.Raycast(rayOrigin, rayDir.normalized, out hit, 100f))
            {
                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    var Heading = dirToTarget;
                    var Distance = Heading.magnitude;
                    if (Distance > 0.01f)
                    {
                        var Direction = Heading / Distance;
                        desiredVelocity = new Vector3(Direction.x * Speed, rb.linearVelocity.y, Direction.z * Speed);
                        transform.forward = new Vector3(Direction.x, 0f, Direction.z);
                    }

                    if (animator != null)
                    {
                        Vector3 flatVel = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
                        animator.SetFloat("Speed_f", flatVel.magnitude);
                        animator.SetBool("Detected", true);
                    }
                }
                else
                {
                    seePlayer = false;
                    desiredVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

                    if (animator != null)
                    {
                        animator.SetBool("Detected", false);
                    }
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        // Применяем желаемую скорость к Rigidbody
        rb.linearVelocity = desiredVelocity;
    }

    private void OnPlayerHitFurniture(Vector3 hitPosition)
    {
        float assumedDuration = 1f;
        ReceiveSound(hitPosition, assumedDuration);
    }

    private void OnPlayerStartedRunning()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= soundDetectionRadius)
        {
            ReceiveSound(player.position, 3f);
        }
    }
}
