using UnityEngine;

public class LevelDesignManager : MonoBehaviour
{
    [SerializeField] private GameObject levelDesign; // Ссылка на объект LevelDesign

    private void Start()
    {
        if (levelDesign == null)
        {
            Debug.LogError("LevelDesignManager: Объект LevelDesign не назначен!");
            return;
        }

        // Проходим по всем дочерним объектам LevelDesign
        foreach (Transform child in levelDesign.transform)
        {
            // Проверяем, есть ли уже FurnitureHitDetector
            if (child.gameObject.GetComponent<FurnitureHitDetector>() == null)
            {
                // Добавляем FurnitureHitDetector
                child.gameObject.AddComponent<FurnitureHitDetector>();
                child.gameObject.AddComponent<Rigidbody>();
            }
        }

        Debug.Log("LevelDesignManager: FurnitureHitDetector добавлен на все объекты внутри LevelDesign.");
    }
}
