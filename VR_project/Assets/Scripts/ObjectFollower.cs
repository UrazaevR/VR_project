using UnityEngine;

public class ObjectFollower : MonoBehaviour
{
    public enum FollowMode
    {
        PositionOnly,
        RotationOnly,
        PositionAndRotation,
        PositionRotationAndScale
    }

    [Header("Target Settings")]
    public Transform target; // Целевой объект для синхронизации
    public FollowMode followMode = FollowMode.PositionAndRotation;

    [Header("Position Settings")]
    public bool useLocalPosition = false;
    public Vector3 positionOffset = Vector3.zero;
    public float positionSmoothTime = 0.1f; // Время сглаживания (0 = мгновенно)

    [Header("Rotation Settings")]
    public bool useLocalRotation = false;
    public Vector3 rotationOffset = Vector3.zero;
    public float rotationSmoothTime = 0.1f; // Время сглаживания (0 = мгновенно)

    [Header("Scale Settings")]
    public bool matchScale = false;
    public float scaleSmoothTime = 0.1f;
    public Vector3 scaleMultiplier = Vector3.one;

    // Приватные переменные для сглаживания
    private Vector3 positionVelocity;
    private Vector3 rotationVelocity;
    private Vector3 scaleVelocity;
    private Quaternion targetRotation;

    void LateUpdate()
    {
        if (target == null) return;

        // Синхронизация позиции
        if (followMode == FollowMode.PositionOnly ||
            followMode == FollowMode.PositionAndRotation ||
            followMode == FollowMode.PositionRotationAndScale)
        {
            Vector3 targetPosition = useLocalPosition ?
                target.localPosition :
                target.position;

            targetPosition += positionOffset;

            if (positionSmoothTime > 0)
            {
                transform.position = useLocalPosition ?
                    Vector3.SmoothDamp(transform.localPosition, targetPosition, ref positionVelocity, positionSmoothTime) :
                    Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, positionSmoothTime);
            }
            else
            {
                if (useLocalPosition) transform.localPosition = targetPosition;
                else transform.position = targetPosition;
            }
        }

        // Синхронизация поворота
        if (followMode == FollowMode.RotationOnly ||
            followMode == FollowMode.PositionAndRotation ||
            followMode == FollowMode.PositionRotationAndScale)
        {
            Quaternion targetRot = useLocalRotation ?
                target.localRotation :
                target.rotation;

            targetRot *= Quaternion.Euler(rotationOffset);

            if (rotationSmoothTime > 0)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    Time.deltaTime / rotationSmoothTime
                );
            }
            else
            {
                if (useLocalRotation) transform.localRotation = targetRot;
                else transform.rotation = targetRot;
            }
        }

        // Синхронизация масштаба
        if (followMode == FollowMode.PositionRotationAndScale && matchScale)
        {
            Vector3 targetScale = Vector3.Scale(target.localScale, scaleMultiplier);

            if (scaleSmoothTime > 0)
            {
                transform.localScale = Vector3.SmoothDamp(
                    transform.localScale,
                    targetScale,
                    ref scaleVelocity,
                    scaleSmoothTime
                );
            }
            else
            {
                transform.localScale = targetScale;
            }
        }
    }

    // Метод для смены цели во время выполнения
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        positionVelocity = Vector3.zero;
        rotationVelocity = Vector3.zero;
        scaleVelocity = Vector3.zero;
    }

    // Метод для мгновенной синхронизации
    public void SnapToTarget()
    {
        if (target == null) return;

        if (useLocalPosition) transform.localPosition = target.localPosition + positionOffset;
        else transform.position = target.position + positionOffset;

        Quaternion targetRot = useLocalRotation ? target.localRotation : target.rotation;
        if (useLocalRotation) transform.localRotation = targetRot * Quaternion.Euler(rotationOffset);
        else transform.rotation = targetRot * Quaternion.Euler(rotationOffset);

        if (matchScale) transform.localScale = Vector3.Scale(target.localScale, scaleMultiplier);
    }
}