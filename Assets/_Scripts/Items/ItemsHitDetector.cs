using UnityEngine;

public class FurnitureHitDetector : MonoBehaviour
{
    // «апоминаем, был ли уже удар
    private bool wasHit = false;

    // —сылка на аудиоклип удара
    [SerializeField] private AudioClip hitSound;

    // ƒл€ примера: публичное свойство, чтобы другие скрипты могли узнать о столкновении
    public bool WasHit => wasHit;

    private void OnCollisionEnter(Collision collision)
    {
        // ѕровер€ем, что столкновение с игроком и ранее не было удара
        if (!wasHit && collision.gameObject.CompareTag("Player"))
        {
            wasHit = true;

            // ”ведомл€ем врагов о столкновении
            EventBus.Instance?.TriggerPlayerHitFurniture(transform.position);

            // ¬оспроизвести звук удара
            var audioManager = FindObjectOfType<AudioManager>();
            if (audioManager != null && hitSound != null)
            {
                audioManager.PlaySFX(hitSound);
            }
        }
    }
}
