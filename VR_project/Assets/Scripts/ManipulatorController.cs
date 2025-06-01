using UnityEngine;

public class ManipulatorController : MonoBehaviour
{
    [Header("Control chair")]
    public ChairInteraction chair;
    [Header("Shoulder Control")]
    public Transform shoulder;
    public float shoulderRotationSpeed = 30f;

    [Header("Arm Control")]
    public Transform arm;
    public Transform forearmPivot;
    public float armMovementSpeed = 20f;
    private float _currentArmAngle = 0f;
    private float _currentForearmAngle = 0f;

    [Header("Wrist Control")]
    public Transform wristPivot;
    public float wristMovementSpeed = 20f;
    private float _currentWristAngle = 0f;

    [Header("Grip Settings")]
    public float grabRadius = 0.1f;
    public Transform gripCenterPoint;
    public float gripHoldDistance = 0.05f;

    // Finger pivots (не изменяемый блок)
    public Transform ProximalPivot_1; public Transform ProximalPivot_2;
    public Transform ProximalPivot_3; public Transform ProximalPivot_4;
    public Transform MiddlePivot_1; public Transform MiddlePivot_2;
    public Transform MiddlePivot_3; public Transform MiddlePivot_4;
    public Transform DistalPivot_1; public Transform DistalPivot_2;
    public Transform DistalPivot_3; public Transform DistalPivot_4;

    private bool _isGripClosed = false; // false = открыто, true = закрыто
    private GameObject _grabbedObject;
    private float _gripState = 0f; // 0 = полностью открыто, 1 = полностью закрыто
    private const float MAX_ARM_ANGLE = 41f;
    private const float MAX_FOREARM_ANGLE = 25f;
    private const float MAX_WRIST_ANGLE = 15f;

    void Update()
    {
        if (chair.isSitting)
        {
            HandleShoulderRotation();
            HandleArmMovement();
            HandleGrip();
        }
    }

    private void HandleShoulderRotation()
    {
        // Плавный поворот вправо (J)
        if (Input.GetKey(KeyCode.J))
        {
            shoulder.Rotate(0, 0, shoulderRotationSpeed * Time.deltaTime);
        }
        // Плавный поворот влево (G)
        if (Input.GetKey(KeyCode.G))
        {
            shoulder.Rotate(0, 0, -shoulderRotationSpeed * Time.deltaTime);
        }
    }

    private void HandleArmMovement()
    {
        // Плавное опускание манипулятора (Y)
        if (Input.GetKey(KeyCode.Y))
        {
            _currentArmAngle = Mathf.MoveTowards(_currentArmAngle, -MAX_ARM_ANGLE, armMovementSpeed * Time.deltaTime);
            _currentForearmAngle = Mathf.MoveTowards(_currentForearmAngle, -MAX_FOREARM_ANGLE, armMovementSpeed * Time.deltaTime);
            _currentWristAngle = Mathf.MoveTowards(_currentWristAngle, MAX_WRIST_ANGLE, wristMovementSpeed * Time.deltaTime);
        }
        // Плавное поднимание манипулятора (H)
        else if (Input.GetKey(KeyCode.H))
        {
            _currentArmAngle = Mathf.MoveTowards(_currentArmAngle, MAX_ARM_ANGLE, armMovementSpeed * Time.deltaTime);
            _currentForearmAngle = Mathf.MoveTowards(_currentForearmAngle, MAX_FOREARM_ANGLE, armMovementSpeed * Time.deltaTime);
            _currentWristAngle = Mathf.MoveTowards(_currentWristAngle, -MAX_WRIST_ANGLE, wristMovementSpeed * Time.deltaTime);
        }

        // Применяем вычисленные углы
        arm.localRotation = Quaternion.Euler(_currentArmAngle, 0, 0);
        forearmPivot.localRotation = Quaternion.Euler(_currentForearmAngle, 0, 0);
        wristPivot.localRotation = Quaternion.Euler(_currentWristAngle, 0, 0);
    }

    private void HandleGrip()
    {
        // Захват/отпускание (U)
        if (Input.GetKeyDown(KeyCode.U))
        {
            _isGripClosed = !_isGripClosed; // Переключаем состояние захвата

            // Если закрываем захват и ничего не держим - пробуем схватить
            if (_isGripClosed && _grabbedObject == null)
            {
                TryGrabObject();
            }
            // Если открываем захват и что-то держим - отпускаем
            else if (!_isGripClosed && _grabbedObject != null)
            {
                ReleaseObject();
            }
        }

        // Плавное изменение состояния захвата
        _gripState = Mathf.MoveTowards(_gripState, _isGripClosed ? 1f : 0f, Time.deltaTime * 3f);
        UpdateFingersRotation(); 
    }

    private void TryGrabObject()
    {
        Collider[] hitColliders = Physics.OverlapSphere(gripCenterPoint.position, grabRadius);
        foreach (var hit in hitColliders)
        {
            if (hit.GetComponent<Rigidbody>() != null && hit.gameObject != this.gameObject)
            {
                GrabObject(hit.gameObject);
                break;
            }
        }
    }

    private void GrabObject(GameObject obj)
    {
        _grabbedObject = obj;

        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Для плавного движения
        }

        // Отключаем коллайдеры объекта при захвате
        /*var colliders = obj.GetComponents<Collider>();
        foreach (var coll in colliders)
        {
            coll.enabled = false;
        }*/

        // Делаем объект дочерним к точке захвата
        obj.transform.SetParent(gripCenterPoint);
        obj.transform.localPosition = Vector3.forward * gripHoldDistance;
        obj.transform.localRotation = Quaternion.identity;
    }

    private void ReleaseObject()
    {
        if (_grabbedObject == null) return;

        // Возвращаем объект в корень иерархии
        _grabbedObject.transform.SetParent(null);

        var rb = _grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            // Сохраняем текущую скорость манипулятора
            rb.velocity = GetComponent<Rigidbody>().velocity;
        }

        // Включаем коллайдеры обратно
        var colliders = _grabbedObject.GetComponents<Collider>();
        foreach (var coll in colliders)
        {
            coll.enabled = true;
        }

        _grabbedObject = null;
    }

    void FixedUpdate()
    {
        // Плавное перемещение захваченного объекта
        if (_grabbedObject != null)
        {
            _grabbedObject.transform.position = Vector3.Lerp(
                _grabbedObject.transform.position,
                gripCenterPoint.TransformPoint(Vector3.forward * gripHoldDistance),
                10f * Time.deltaTime);

            _grabbedObject.transform.rotation = Quaternion.Lerp(
                _grabbedObject.transform.rotation,
                gripCenterPoint.rotation,
                10f * Time.deltaTime);
        }
    }

    private void UpdateFingersRotation()
    {
        // Proximal phalanges
        ProximalPivot_1.localRotation = Quaternion.Euler(_gripState * 30, 0, 0);
        ProximalPivot_2.localRotation = Quaternion.Euler(0, _gripState * 30, 0);
        ProximalPivot_3.localRotation = Quaternion.Euler(-_gripState * 30, 0, 0);
        ProximalPivot_4.localRotation = Quaternion.Euler(0, -_gripState * 30, 0);

        // Middle phalanges
        MiddlePivot_1.localRotation = Quaternion.Euler(_gripState * 20, 0, 0);
        MiddlePivot_2.localRotation = Quaternion.Euler(0, _gripState * 20, 0);
        MiddlePivot_3.localRotation = Quaternion.Euler(-_gripState * 20, 0, 0);
        MiddlePivot_4.localRotation = Quaternion.Euler(0, -_gripState * 20, 0);

        // Distal phalanges
        DistalPivot_1.localRotation = Quaternion.Euler(_gripState * 10, 0, 0);
        DistalPivot_2.localRotation = Quaternion.Euler(0, _gripState * 10, 0);
        DistalPivot_3.localRotation = Quaternion.Euler(-_gripState * 10, 0, 0);
        DistalPivot_4.localRotation = Quaternion.Euler(0, -_gripState * 10, 0);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gripCenterPoint.position, grabRadius);
    }
}