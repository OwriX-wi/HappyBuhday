using UnityEditor;
using UnityEngine;

public class LevelDesignEditor : MonoBehaviour
{
    [MenuItem("Tools/Add FurnitureHitDetector to LevelDesign")]
    private static void AddFurnitureHitDetector()
    {
        // Находим объект LevelDesign в сцене
        GameObject levelDesign = GameObject.Find("LevelDesign");
        if (levelDesign == null)
        {
            Debug.LogError("LevelDesignEditor: Объект LevelDesign не найден в сцене!");
            return;
        }

        // Проходим по всем дочерним объектам LevelDesign
        foreach (Transform child in levelDesign.transform)
        {
            // Проверяем, есть ли уже FurnitureHitDetector
            if (child.gameObject.GetComponent<FurnitureHitDetector>() == null)
            {
                // Добавляем FurnitureHitDetector
                Undo.AddComponent<FurnitureHitDetector>(child.gameObject);
            }
        }

        Debug.Log("LevelDesignEditor: FurnitureHitDetector добавлен на все объекты внутри LevelDesign.");
    }
}