/*using UnityEngine;

public class ManipulatorController : MonoBehaviour
{
    [Header("Base Settings")]
    public Transform baseRoot;

    [Header("Shoulder")]
    public Transform shoulder;
    public float shoulderRotationSpeed = 30f;

    [Header("Arm")]
    public Transform arm;
    public float armRotationSpeed = 20f;
    public float minArmAngle = -45f;
    public float maxArmAngle = 90f;
    private float _currentArmAngle;

    [Header("Forearm")]
    public Transform forearmPivot;
    public float forearmRotationSpeed = 20f;
    private float _currentForearmAngle;

    [Header("Wrist")]
    public Transform wristPivot;
    public float wristRotationSpeed = 15f;
    private float _currentWristAngle;

    [Header("Grip Settings")]
    public Transform ProximalPivot_1; public Transform ProximalPivot_2;
    public Transform ProximalPivot_3; public Transform ProximalPivot_4;

    public Transform MiddlePivot_1; public Transform MiddlePivot_2;
    public Transform MiddlePivot_3; public Transform MiddlePivot_4;

    public Transform DistalPivot_1; public Transform DistalPivot_2;
    public Transform DistalPivot_3; public Transform DistalPivot_4;

    public float gripSpeed = 30f;
    public float maxGripAngle = 30f;
    private float _currentGripState;
    private bool _isGripping;
    private GameObject _grabbedObject;
    private bool _canGrab = true;

    [Header("Grab Settings")]
    public float grabRadius = 0.1f;
    public LayerMask grabLayerMask;
    public Transform gripCenterPoint;
    public float gripHoldDistance = 0.05f;
    public Vector3 gripRotationOffset = Vector3.zero;

    void Update()
    {
        HandleBaseMovement();
        HandleGrip();
        CheckForGrabbableObjects();
        UpdateGrabbedObjectPosition();
    }

    private void HandleBaseMovement()
    {
        // Shoulder Rotation
        if (Input.GetKey(KeyCode.U))
            shoulder.Rotate(0, 0, shoulderRotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.O))
            shoulder.Rotate(0, 0, -shoulderRotationSpeed * Time.deltaTime);

        // Arm Movement
        if (Input.GetKey(KeyCode.I))
            _currentArmAngle = Mathf.MoveTowards(_currentArmAngle, maxArmAngle, armRotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.K))
            _currentArmAngle = Mathf.MoveTowards(_currentArmAngle, minArmAngle, armRotationSpeed * Time.deltaTime);
        arm.localRotation = Quaternion.Euler(_currentArmAngle, 0, 0);

        // Forearm Rotation
        if (Input.GetKey(KeyCode.J))
            _currentForearmAngle = Mathf.MoveTowards(_currentForearmAngle, 90f, forearmRotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.L))
            _currentForearmAngle = Mathf.MoveTowards(_currentForearmAngle, -90f, forearmRotationSpeed * Time.deltaTime);
        forearmPivot.localRotation = Quaternion.Euler(_currentForearmAngle, 0, 0);

        // Wrist Rotation
        if (Input.GetKey(KeyCode.N))
            _currentWristAngle = Mathf.MoveTowards(_currentWristAngle, 45f, wristRotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.M))
            _currentWristAngle = Mathf.MoveTowards(_currentWristAngle, -45f, wristRotationSpeed * Time.deltaTime);
        wristPivot.localRotation = Quaternion.Euler(_currentWristAngle, 0, 0);
    }

    private void HandleGrip()
    {
        if (Input.GetKey(KeyCode.Y))
        {
            _isGripping = true;
            _currentGripState = Mathf.MoveTowards(_currentGripState, 1f, gripSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.H))
        {
            _isGripping = false;
            _currentGripState = Mathf.MoveTowards(_currentGripState, 0f, gripSpeed * Time.deltaTime);
            if (_grabbedObject != null) ReleaseObject();
        }

        UpdateFingersRotation();
    }

    private void CheckForGrabbableObjects()
    {
        if (_isGripping && _grabbedObject == null && _currentGripState > 0.8f && _canGrab)
        {
            Collider[] hitColliders = Physics.OverlapSphere(gripCenterPoint.position, grabRadius, grabLayerMask);
            foreach (var hit in hitColliders)
            {
                if (hit.CompareTag("Grabbable"))
                {
                    GrabObject(hit.gameObject);
                    break;
                }
            }
        }
    }

    private void GrabObject(GameObject obj)
    {
        _grabbedObject = obj;
        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }

        obj.transform.SetParent(gripCenterPoint);
        obj.transform.localPosition = Vector3.forward * gripHoldDistance;
        obj.transform.localRotation = Quaternion.Euler(gripRotationOffset);
        _canGrab = false;
    }

    private void UpdateGrabbedObjectPosition()
    {
        if (_grabbedObject != null)
        {
            // Плавное перемещение к точке захвата
            _grabbedObject.transform.position = Vector3.Lerp(
                _grabbedObject.transform.position,
                gripCenterPoint.TransformPoint(Vector3.forward * gripHoldDistance),
                10f * Time.deltaTime);

            // Проверка дистанции для авто-отпускания
            if (Vector3.Distance(_grabbedObject.transform.position, gripCenterPoint.position) > grabRadius * 2f)
            {
                ReleaseObject();
            }
        }
    }

    private void UpdateFingersRotation()
    {
        // Отрицательные углы обеспечивают только движение "на захват"
        float proximalAngle = Mathf.Clamp(_currentGripState * maxGripAngle, 0, maxGripAngle);
        float middleAngle = Mathf.Clamp(_currentGripState * maxGripAngle * 0.7f, 0, maxGripAngle * 0.7f);
        float distalAngle = Mathf.Clamp(_currentGripState * maxGripAngle * 0.5f, 0, maxGripAngle * 0.5f);

        // Proximal phalanges
        ProximalPivot_1.localRotation = Quaternion.Euler(proximalAngle, 0, 0);
        ProximalPivot_2.localRotation = Quaternion.Euler(0, proximalAngle, 0);
        ProximalPivot_3.localRotation = Quaternion.Euler(-proximalAngle, 0, 0);
        ProximalPivot_4.localRotation = Quaternion.Euler(0, -proximalAngle, 0);

        // Middle phalanges
        MiddlePivot_1.localRotation = Quaternion.Euler(middleAngle, 0, 0);
        MiddlePivot_2.localRotation = Quaternion.Euler(0, middleAngle, 0);
        MiddlePivot_3.localRotation = Quaternion.Euler(-middleAngle, 0, 0);
        MiddlePivot_4.localRotation = Quaternion.Euler(0, -middleAngle, 0);

        // Distal phalanges
        DistalPivot_1.localRotation = Quaternion.Euler(distalAngle, 0, 0);
        DistalPivot_2.localRotation = Quaternion.Euler(0, distalAngle, 0);
        DistalPivot_3.localRotation = Quaternion.Euler(-distalAngle, 0, 0);
        DistalPivot_4.localRotation = Quaternion.Euler(0, -distalAngle, 0);
    }

    private void ReleaseObject()
    {
        if (_grabbedObject == null) return;

        _grabbedObject.transform.SetParent(null);
        if (_grabbedObject.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
        }
        _grabbedObject = null;
        _canGrab = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gripCenterPoint != null ? gripCenterPoint.position : wristPivot.position, grabRadius);
    }

    public bool TryGrabObject(GameObject obj)
    {
        if (obj == null || _grabbedObject != null || !_canGrab || !obj.CompareTag("Grabbable"))
            return false;

        GrabObject(obj);
        return true;
    }
}*/

// С захватом
/*using UnityEngine;

public class ManipulatorController : MonoBehaviour
{
    [Header("Base Settings")]
    public Transform baseRoot;

    [Header("Shoulder")]
    public Transform shoulder;
    public float shoulderRotationSpeed = 30f;

    [Header("Arm")]
    public Transform arm;
    public float armRotationSpeed = 20f;
    public float minArmAngle = -45f;
    public float maxArmAngle = 90f;
    private float _currentArmAngle;

    [Header("Forearm")]
    public Transform forearmPivot;
    public float forearmRotationSpeed = 20f;
    private float _currentForearmAngle;

    [Header("Wrist")]
    public Transform wristPivot;
    public float wristRotationSpeed = 15f;
    private float _currentWristAngle;

    [Header("Grip Settings")]
    public Transform ProximalPivot_1; public Transform ProximalPivot_2;
    public Transform ProximalPivot_3; public Transform ProximalPivot_4;

    public Transform MiddlePivot_1; public Transform MiddlePivot_2;
    public Transform MiddlePivot_3; public Transform MiddlePivot_4;

    public Transform DistalPivot_1; public Transform DistalPivot_2;
    public Transform DistalPivot_3; public Transform DistalPivot_4;

    public float gripSpeed = 30f;
    public float maxGripAngle = 30f;
    private float _currentGripState;
    private bool _isGripping;
    private GameObject _grabbedObject;
    private bool _canGrab = true;

    [Header("Grab Settings")]
    public float grabRadius = 0.1f;
    public LayerMask grabLayerMask;
    public Transform gripCenterPoint;
    public float gripHoldDistance = 0.05f;
    public Vector3 gripRotationOffset = Vector3.zero;

    void Update()
    {
        HandleBaseMovement();
        HandleGrip();
        CheckForGrabbableObjects();
        UpdateGrabbedObjectPosition();
    }

    private void HandleBaseMovement()
    {
        // Shoulder Rotation
        if (Input.GetKey(KeyCode.U))
            shoulder.Rotate(0, 0, shoulderRotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.O))
            shoulder.Rotate(0, 0, -shoulderRotationSpeed * Time.deltaTime);

        // Arm Movement
        if (Input.GetKey(KeyCode.I))
            _currentArmAngle = Mathf.MoveTowards(_currentArmAngle, maxArmAngle, armRotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.K))
            _currentArmAngle = Mathf.MoveTowards(_currentArmAngle, minArmAngle, armRotationSpeed * Time.deltaTime);
        arm.localRotation = Quaternion.Euler(_currentArmAngle, 0, 0);

        // Forearm Rotation
        if (Input.GetKey(KeyCode.J))
            _currentForearmAngle = Mathf.MoveTowards(_currentForearmAngle, 90f, forearmRotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.L))
            _currentForearmAngle = Mathf.MoveTowards(_currentForearmAngle, -90f, forearmRotationSpeed * Time.deltaTime);
        forearmPivot.localRotation = Quaternion.Euler(_currentForearmAngle, 0, 0);

        // Wrist Rotation
        if (Input.GetKey(KeyCode.N))
            _currentWristAngle = Mathf.MoveTowards(_currentWristAngle, 45f, wristRotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.M))
            _currentWristAngle = Mathf.MoveTowards(_currentWristAngle, -45f, wristRotationSpeed * Time.deltaTime);
        wristPivot.localRotation = Quaternion.Euler(_currentWristAngle, 0, 0);
    }

    private void HandleGrip()
    {
        if (Input.GetKey(KeyCode.Y))
        {
            _isGripping = true;
            _currentGripState = Mathf.MoveTowards(_currentGripState, 1f, gripSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.H))
        {
            _isGripping = false;
            _currentGripState = Mathf.MoveTowards(_currentGripState, 0f, gripSpeed * Time.deltaTime);
            if (_grabbedObject != null) ReleaseObject();
        }

        UpdateFingersRotation();
    }

    private void CheckForGrabbableObjects()
    {
        if (_isGripping && _grabbedObject == null && _currentGripState > 0.8f && _canGrab)
        {
            Collider[] hitColliders = Physics.OverlapSphere(gripCenterPoint.position, grabRadius);
            foreach (var hit in hitColliders)
            {
                // Проверяем либо тег Grabbable, либо наличие Rigidbody
                if (hit.CompareTag("Grabbable") || hit.GetComponent<Rigidbody>() != null)
                {
                    GrabObject(hit.gameObject);
                    break;
                }
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
        }

        // Отключаем коллайдеры объекта при захвате
        var colliders = obj.GetComponents<Collider>();
        foreach (var coll in colliders)
        {
            coll.enabled = false;
        }

        obj.transform.SetParent(gripCenterPoint);
        obj.transform.localPosition = Vector3.forward * gripHoldDistance;
        obj.transform.localRotation = Quaternion.Euler(gripRotationOffset);
        _canGrab = false;
    }

    private void UpdateGrabbedObjectPosition()
    {
        if (_grabbedObject != null)
        {
            // Плавное перемещение к точке захвата
            _grabbedObject.transform.position = Vector3.Lerp(
                _grabbedObject.transform.position,
                gripCenterPoint.TransformPoint(Vector3.forward * gripHoldDistance),
                10f * Time.deltaTime);

            // Проверка дистанции для авто-отпускания
            if (Vector3.Distance(_grabbedObject.transform.position, gripCenterPoint.position) > grabRadius * 2f)
            {
                ReleaseObject();
            }
        }
    }

    private void UpdateFingersRotation()
    {
        // Отрицательные углы обеспечивают только движение "на захват"
        float proximalAngle = Mathf.Clamp(_currentGripState * maxGripAngle, 0, maxGripAngle);
        float middleAngle = Mathf.Clamp(_currentGripState * maxGripAngle * 0.7f, 0, maxGripAngle * 0.7f);
        float distalAngle = Mathf.Clamp(_currentGripState * maxGripAngle * 0.5f, 0, maxGripAngle * 0.5f);

        // Proximal phalanges
        ProximalPivot_1.localRotation = Quaternion.Euler(proximalAngle, 0, 0);
        ProximalPivot_2.localRotation = Quaternion.Euler(0, proximalAngle, 0);
        ProximalPivot_3.localRotation = Quaternion.Euler(-proximalAngle, 0, 0);
        ProximalPivot_4.localRotation = Quaternion.Euler(0, -proximalAngle, 0);

        // Middle phalanges
        MiddlePivot_1.localRotation = Quaternion.Euler(middleAngle, 0, 0);
        MiddlePivot_2.localRotation = Quaternion.Euler(0, middleAngle, 0);
        MiddlePivot_3.localRotation = Quaternion.Euler(-middleAngle, 0, 0);
        MiddlePivot_4.localRotation = Quaternion.Euler(0, -middleAngle, 0);

        // Distal phalanges
        DistalPivot_1.localRotation = Quaternion.Euler(distalAngle, 0, 0);
        DistalPivot_2.localRotation = Quaternion.Euler(0, distalAngle, 0);
        DistalPivot_3.localRotation = Quaternion.Euler(-distalAngle, 0, 0);
        DistalPivot_4.localRotation = Quaternion.Euler(0, -distalAngle, 0);
    }

    private void ReleaseObject()
    {
        if (_grabbedObject == null) return;

        _grabbedObject.transform.SetParent(null);
        var rb = _grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Включаем коллайдеры обратно
        var colliders = _grabbedObject.GetComponents<Collider>();
        foreach (var coll in colliders)
        {
            coll.enabled = true;
        }

        _grabbedObject = null;
        _canGrab = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gripCenterPoint != null ? gripCenterPoint.position : wristPivot.position, grabRadius);
    }

    public bool TryGrabObject(GameObject obj)
    {
        if (obj == null || _grabbedObject != null || !_canGrab || !obj.CompareTag("Grabbable"))
            return false;

        GrabObject(obj);
        return true;
    }
}*/