using System.Collections.Generic;
using UnityEngine;

public class LentaScript : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5.0f; // Увеличил скорость для лучшей видимости

    [Header("Direction Points")]
    public Transform startPoint;
    public Transform endPoint;

    private Vector3 movementDirection;
    private List<Rigidbody> objectsOnBelt = new List<Rigidbody>();
    private Collider beltCollider;

    void Start()
    {
        beltCollider = GetComponent<Collider>();
        if (beltCollider == null)
        {
            Debug.LogError("No collider found on the belt! Adding BoxCollider...");
            beltCollider = gameObject.AddComponent<BoxCollider>();
        }

        // Критически важно: коллайдер НЕ должен быть триггером!
        beltCollider.isTrigger = false;

        if (startPoint == null || endPoint == null)
        {
            Debug.LogError("Assign start and end points in inspector!");
            enabled = false;
        }
    }

    void FixedUpdate()
    {
        if (startPoint == null || endPoint == null) return;

        movementDirection = (endPoint.position - startPoint.position).normalized;

        foreach (Rigidbody rb in objectsOnBelt)
        {
            if (rb != null)
            {
                // Используем физический метод для движения
                MoveObject(rb);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.collider.attachedRigidbody;
        if (rb != null && !rb.isKinematic && !objectsOnBelt.Contains(rb))
        {
            objectsOnBelt.Add(rb);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        Rigidbody rb = collision.collider.attachedRigidbody;
        if (rb != null && objectsOnBelt.Contains(rb))
        {
            objectsOnBelt.Remove(rb);
        }
    }

    void MoveObject(Rigidbody rb)
    {
        // Вычисляем движение в мировых координатах
        Vector3 movement = movementDirection * speed * Time.fixedDeltaTime;

        // Применяем движение через физический метод
        rb.MovePosition(rb.position + movement);
    }

    void OnDrawGizmosSelected()
    {
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPoint.position, 0.15f);
            Gizmos.DrawSphere(endPoint.position, 0.15f);
            Gizmos.DrawLine(startPoint.position, endPoint.position);

            // Стрелка направления
            Vector3 dir = (endPoint.position - startPoint.position).normalized;
            Gizmos.DrawRay(endPoint.position, -dir * 0.5f);
            GizmoUtils.DrawArrow(endPoint.position, dir, 0.5f, 20f);
        }
    }
}

// Вспомогательный класс для рисования стрелки
public static class GizmoUtils
{
    public static void DrawArrow(Vector3 pos, Vector3 direction, float length, float arrowAngle)
    {
        Vector3 arrowEnd = pos + direction * length;
        Gizmos.DrawLine(pos, arrowEnd);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowAngle, 0) * Vector3.forward;

        Gizmos.DrawRay(arrowEnd, right * 0.25f);
        Gizmos.DrawRay(arrowEnd, left * 0.25f);
    }
}