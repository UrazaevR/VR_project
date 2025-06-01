using System.Collections.Generic;
using UnityEngine;

public class LentaScript : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2.0f; // Скорость движения объектов
    public bool isRunning = true;

    [Header("Direction Points")]
    public Transform startPoint; // Стартовая точка направления
    public Transform endPoint;   // Конечная точка направления

    private Vector3 movementDirection; // Направление движения
    private HashSet<Rigidbody> objectsOnBelt = new HashSet<Rigidbody>(); // Объекты на ленте

    void Start()
    {
        // Проверка наличия точек направления
        if (startPoint == null || endPoint == null)
        {
            Debug.LogError("StartPoint and EndPoint must be assigned!");
            enabled = false;
            return;
        }

        // Первоначальный расчет направления
        CalculateDirection();

        // Настройка коллайдера
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = false;
        }
    }

    void FixedUpdate()
    {
        // Обновление направления на случай изменения точек
        CalculateDirection();

        // Перемещение всех объектов на ленте
        if (isRunning)
        {
            foreach (Rigidbody rb in objectsOnBelt)
            {
                if (rb != null)
                {
                    MoveObject(rb);
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb != null && !rb.isKinematic && !objectsOnBelt.Contains(rb))
        {
            objectsOnBelt.Add(rb);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb != null && objectsOnBelt.Contains(rb))
        {
            objectsOnBelt.Remove(rb);
        }
    }

    void CalculateDirection()
    {
        // Расчет нормализованного направления движения
        movementDirection = (endPoint.position - startPoint.position).normalized;
    }

    void MoveObject(Rigidbody rb)
    {
        // Расчет смещения
        Vector3 movement = movementDirection * speed * Time.fixedDeltaTime;

        // Применение смещения к позиции объекта
        rb.MovePosition(rb.position + movement);
    }

    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPoint.position, 0.1f);
            Gizmos.DrawSphere(endPoint.position, 0.1f);
            Gizmos.DrawLine(startPoint.position, endPoint.position);

            // Стрелка направления
            Vector3 dir = (endPoint.position - startPoint.position).normalized;
            Gizmos.DrawRay(endPoint.position, -dir * 0.5f);
        }
    }
}