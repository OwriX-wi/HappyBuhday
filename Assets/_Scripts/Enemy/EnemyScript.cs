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

    void Start()
    {
        Speed = MaxSpeed;

        // Подписываемся на события
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnPlayerHitFurniture += OnPlayerHitFurniture;
            EventBus.Instance.OnPlayerStartedRunning += OnPlayerStartedRunning;
        }
    }

    void OnDestroy()
    {
        // Отписываемся от событий
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnPlayerHitFurniture -= OnPlayerHitFurniture;
            EventBus.Instance.OnPlayerStartedRunning -= OnPlayerStartedRunning;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //detect any players in range
        if (!seePlayer)
        {
            hitColliders = Physics.OverlapSphere(transform.position, 10f);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    target = hitCollider.gameObject;
                    seePlayer = true;
                }
            }
        }
        else
        {
            if (Physics.Raycast(transform.position, target.transform.position - transform.position, out hit, 100f))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    seePlayer = true;
                }
                else
                {
                    var Heading = target.transform.position - transform.position;
                    var Distance = Heading.magnitude;
                    var Direction = Heading / Distance;

                    Vector3 Move = new Vector3(Direction.x * Speed, 0, Direction.z * Speed);
                    rb.linearVelocity = Move;
                    transform.forward = Move;
                }
            }
        }
    }

    private void OnPlayerHitFurniture(Vector3 hitPosition)
    {
        // Проверяем, находится ли звук удара в радиусе обнаружения
        if (Vector3.Distance(transform.position, hitPosition) <= soundDetectionRadius)
        {
            Debug.Log("Enemy: Игрок обнаружен из-за столкновения с мебелью!");
            seePlayer = true;
            target = GameObject.FindGameObjectWithTag("Player");
        }
    }

    private void OnPlayerStartedRunning()
    {
        // Проверяем, находится ли игрок в радиусе обнаружения
        if (player != null && Vector3.Distance(transform.position, player.position) <= soundDetectionRadius)
        {
            Debug.Log("Enemy: Игрок обнаружен из-за бега!");
            seePlayer = true;
            target = GameObject.FindGameObjectWithTag("Player");
        }
    }
}
