using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnScript : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject prefabToSpawn; // Префаб для спавна
    public float spawnInterval = 3.0f; // Интервал между спавном объектов (в секундах)

    void Start()
    {
        // Начинаем спавнить объекты с заданным интервалом
        InvokeRepeating(nameof(SpawnObject), 0f, spawnInterval);
    }

    void SpawnObject()
    {
        // Проверка наличия префаба
        if (prefabToSpawn == null)
        {
            Debug.LogError("Prefab is not assigned in SpawnScript on " + gameObject.name);
            return;
        }

        // Создаем экземпляр объекта внутри текущего объекта
        GameObject spawnedObject = Instantiate(
            prefabToSpawn
        );

        // Делаем текущий объект родителем для нового объекта
        spawnedObject.transform.SetParent(transform);

        // Сохраняем локальные координаты (если нужно)
        spawnedObject.transform.localPosition = Vector3.zero;
        spawnedObject.transform.localRotation = Quaternion.identity;
    }

    // Опционально: функция для изменения интервала во время выполнения
    public void SetSpawnInterval(float newInterval)
    {
        // Отменяем предыдущий вызов
        CancelInvoke(nameof(SpawnObject));

        // Устанавливаем новый интервал
        spawnInterval = newInterval;

        // Перезапускаем спавн
        InvokeRepeating(nameof(SpawnObject), 0f, spawnInterval);
    }
}