using UnityEngine;
using Assets._Scripts.Enemy;


public class EnemyScript : MonoBehaviour
{
    public float MaxSpeed;
    public float Speed;

    public Rigidbody rb;
    public GameObject target;
    public Transform player;
    public Animator animator;

    private Vector3 desiredVelocity = Vector3.zero;

    [SerializeField] private DetectionComponent detection;

    void Start()
    {
        Speed = MaxSpeed;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (player == null)
        {
            var pgo = GameObject.FindGameObjectWithTag("Player");
            if (pgo != null)
                player = pgo.transform;
        }

        if (detection != null)
        {
            detection.animator = animator;
            detection.rb = rb;
            detection.player = player;
        }
        else
        {
            Debug.LogError("DetectionComponent не найден на объекте " + gameObject.name);
        }

        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnPlayerHitFurniture += OnPlayerHitFurniture;
            EventBus.Instance.OnPlayerStartedRunning += OnPlayerStartedRunning;
            try
            {
                EventBus.Instance.OnPlayerMadeSound += ReceiveSound;
            }
            catch { }
        }
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

    public void ReceiveSound(Vector3 pos, float duration)
    {
        if (detection != null)
            detection.ReceiveSound(pos, duration);
    }

    void Update()
    {
        Debug.Log("desiredVelocity: " + desiredVelocity);
        if (rb == null || detection == null) return;

        else if (detection.IsPatrolling)
        {
            Vector3 patrolDir = detection.GetPatrolDirection();
            detection.UpdatePatrolPointIfNeeded();
            desiredVelocity = new Vector3(patrolDir.x * detection.GetPatrolSpeed(), rb.linearVelocity.y, patrolDir.z * detection.GetPatrolSpeed());
            if (animator != null)
            {
                Vector3 flatVel = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
                animator.SetFloat("Speed_f", flatVel.magnitude);
            }
        }

        if (detection.MoveToSoundPosition)
        {
            Vector3 dir = detection.SoundSourcePosition - transform.position;
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

        detection.SightDetection();

        if (detection.SeePlayer && detection.target != null)
        {
            Vector3 dirToTarget = detection.target.transform.position - transform.position;
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
            desiredVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            if (animator != null)
            {
                animator.SetBool("Detected", false);
            }
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        rb.linearVelocity = desiredVelocity;
        Debug.Log("linearVelocity: " + rb.linearVelocity);
    }

    private void OnPlayerHitFurniture(Vector3 hitPosition)
    {
        float assumedDuration = 1f;
        ReceiveSound(hitPosition, assumedDuration);
    }

    private void OnPlayerStartedRunning()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= (detection?.SoundDetectionRadius ?? 15f))
        {
            ReceiveSound(player.position, 3f);
        }
    }
}