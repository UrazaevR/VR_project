using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteOnContact : MonoBehaviour
{
    [Header("Collision Settings")]
    public bool useTrigger = true; // Использовать триггер или физическое столкновение

    [Header("Exclusion Settings")]
    public List<GameObject> excludedObjects = new List<GameObject>(); // Список объектов-исключений
    public bool excludeChildren = true; // Защищать дочерние объекты исключений

    void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;
        ProcessContact(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (useTrigger) return;
        ProcessContact(collision.gameObject);
    }

    void ProcessContact(GameObject contactObject)
    {
        // Проверяем, что объект не в списке исключений
        if (!IsExcluded(contactObject))
        {
            Destroy(contactObject);
        }
    }

    bool IsExcluded(GameObject obj)
    {
        // Проверка прямого совпадения
        if (excludedObjects.Contains(obj)) return true;

        // Проверка дочерних объектов (если включено)
        if (excludeChildren)
        {
            foreach (GameObject excluded in excludedObjects)
            {
                if (excluded != null && obj.transform.IsChildOf(excluded.transform))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Редактор: добавление объектов в список исключений
    [ContextMenu("Add Parent to Exclusions")]
    void AddParentToExclusions()
    {
        if (transform.parent != null && !excludedObjects.Contains(transform.parent.gameObject))
        {
            excludedObjects.Add(transform.parent.gameObject);
            Debug.Log($"Added parent {transform.parent.name} to exclusions");
        }
    }

    // Визуализация исключений
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        foreach (GameObject excluded in excludedObjects)
        {
            if (excluded != null)
            {
                Gizmos.DrawSphere(excluded.transform.position, 0.3f);
                Gizmos.DrawLine(transform.position, excluded.transform.position);
            }
        }
    }
}