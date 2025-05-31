using UnityEngine;

public class DeleteOnContact1 : MonoBehaviour
{
    [Header("Settings")]
    public bool useTrigger = true; // Использовать триггер или физическое столкновение

    void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;
        DestroyObject(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (useTrigger) return;
        DestroyObject(collision.gameObject);
    }

    void DestroyObject(GameObject objToDestroy)
    {
        // Проверяем, что объект не является родителем
        if (objToDestroy != transform.parent?.gameObject)
        {
            Destroy(objToDestroy);
        }
    }
}