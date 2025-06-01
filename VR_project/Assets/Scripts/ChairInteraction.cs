using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class ChairInteraction : MonoBehaviour
{
    [Header("Chair Settings")]
    [Tooltip("Point where the player will sit")]
    public Transform sittingPoint;

    [Tooltip("Key to stand up (Keyboard)")]
    public KeyCode standUpKey = KeyCode.Space;

    [Tooltip("Action to stand up (VR)")]
    public SteamVR_Action_Boolean standUpAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");

    [Header("Player Settings")]
    [Tooltip("Player object")]
    public Player player;

    [Tooltip("Player camera (if not set, will use Player's HMD or Main Camera)")]
    public Transform playerCamera;

    [Tooltip("Sitting height adjustment")]
    public float sittingHeight = 0.7f;

    [Header("Stand Up Settings")]
    [Tooltip("Distance in front of the chair to stand up")]
    public float standUpDistance = 0.5f;

    [Header("Rotation Settings")]
    [Tooltip("Should player rotate to face chair's forward when sitting?")]
    public bool rotateToChairForward = true;

    private bool isSitting = false;
    private CharacterController characterController;
    private Vector3 originalCenter;
    private float originalHeight;
    private bool wasKinematic;
    private Quaternion originalPlayerRotation;
    private Quaternion originalCameraRotation;
    private Vector3 standUpPosition;

    // Components to disable during sitting
    private Behaviour[] locomotionComponents;

    private void Start()
    {
        if (player == null)
        {
            player = Player.instance;
        }

        if (player != null)
        {
            characterController = player.GetComponent<CharacterController>();
            locomotionComponents = player.GetComponentsInChildren<Behaviour>();

            // Initialize player camera
            if (playerCamera == null)
            {
                if (player.hmdTransform != null)
                {
                    playerCamera = player.hmdTransform;
                }
                else
                {
                    playerCamera = Camera.main.transform;
                }
            }
        }
        else
        {
            // Fallback if player not found
            if (playerCamera == null)
            {
                playerCamera = Camera.main.transform;
            }
        }

        if (sittingPoint == null)
        {
            sittingPoint = transform;
        }
    }

    private void Update()
    {
        if (isSitting)
        {
            // Check for stand up input
            bool standUpInput = Input.GetKeyDown(standUpKey) ||
                               (standUpAction != null && standUpAction.GetStateDown(SteamVR_Input_Sources.Any));

            if (standUpInput)
            {
                StandUp();
            }
        }
    }

    // VR interaction
    private void HandHoverUpdate(Hand hand)
    {
        if (hand.GetGrabStarting() != GrabTypes.None && !isSitting)
        {
            SitDown();
        }
    }

    // Mouse click
    private void OnMouseDown()
    {
        if (!isSitting && (Player.instance == null || !Player.instance.isActiveAndEnabled))
        {
            SitDown();
        }
    }

    public void SitDown()
    {
        if (isSitting || player == null) return;

        isSitting = true;

        // Save original parameters
        if (characterController != null)
        {
            originalHeight = characterController.height;
            originalCenter = characterController.center;
        }

        // Save player and camera rotations
        originalPlayerRotation = player.transform.rotation;
        originalCameraRotation = playerCamera.rotation;

        // Calculate stand up position relative to camera direction
        CalculateStandUpPosition();

        // Disable movement
        DisableMovement();

        // Move player to chair relative to camera
        MovePlayerToChair();

        // Rotate player to face chair's direction
        if (rotateToChairForward)
        {
            RotatePlayerToChair();
        }

        // Adjust sitting position
        SetupSittingPosition();

        Debug.Log("Player sat down");
    }

    private void CalculateStandUpPosition()
    {
        // Calculate direction based on camera orientation
        Vector3 cameraForward = playerCamera.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        standUpPosition = sittingPoint.position + cameraForward * standUpDistance;
        standUpPosition.y = sittingPoint.position.y;
    }

    private void RotatePlayerToChair()
    {
        // Get chair's forward direction
        Vector3 chairForward = sittingPoint.forward;
        chairForward.y = 0;

        if (chairForward != Vector3.zero)
        {
            // Calculate rotation difference between camera and chair
            Quaternion targetRotation = Quaternion.LookRotation(chairForward);
            Quaternion cameraRotationDifference = targetRotation * Quaternion.Inverse(playerCamera.rotation);

            // Apply rotation to player
            var rotation = cameraRotationDifference * player.transform.rotation;
            Debug.Log(rotation.y);
            rotation.y = rotation.y - 0.7075f;
            Debug.Log(rotation.y);
            player.transform.rotation = rotation;
            Debug.Log(rotation);

        }
    }

    private void MovePlayerToChair()
    {
        // Calculate camera offset relative to player
        Vector3 cameraOffset = playerCamera.position - player.transform.position;
        cameraOffset.y = 0; // Keep only horizontal offset

        // Position player so camera is above sitting point
        player.transform.position = sittingPoint.position - cameraOffset;
    }

    private void SetupSittingPosition()
    {
        if (characterController != null)
        {
            // Adjust character controller for sitting
            characterController.height = sittingHeight;
            characterController.center = new Vector3(0, sittingHeight / 2f, 0);
        }

        // Adjust player height to sitting point while maintaining camera position
        Vector3 cameraPosBefore = playerCamera.position;
        player.transform.position = new Vector3(
            player.transform.position.x,
            sittingPoint.position.y,
            player.transform.position.z
        );

        // Compensate for any camera movement
        Vector3 cameraOffset = playerCamera.position - cameraPosBefore;
        player.transform.position -= cameraOffset;
    }

    private void DisableMovement()
    {
        // Disable locomotion components
        foreach (var component in locomotionComponents)
        {
            if (component != null && component != this)
            {
                // Skip essential components
                if (component is Camera ||
                    component is AudioListener ||
                    component is SteamVR_Camera ||
                    component is Transform ||
                    component is Hand ||
                    component is Player)
                {
                    continue;
                }

                component.enabled = false;
            }
        }

        // Handle Rigidbody
        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
        if (playerRigidbody != null)
        {
            wasKinematic = playerRigidbody.isKinematic;
            playerRigidbody.isKinematic = true;
        }
    }

    private void EnableMovement()
    {
        // Enable locomotion components
        foreach (var component in locomotionComponents)
        {
            if (component != null && component != this)
            {
                component.enabled = true;
            }
        }

        // Restore Rigidbody state
        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = wasKinematic;
        }
    }

    public void StandUp()
    {
        if (!isSitting || player == null) return;

        isSitting = false;

        // Restore character controller
        if (characterController != null)
        {
            characterController.height = originalHeight;
            characterController.center = originalCenter;
        }

        // Move player to stand position
        player.transform.position = standUpPosition;

        // Restore original rotation
        player.transform.rotation = originalPlayerRotation;
        playerCamera.rotation = originalCameraRotation;

        // Adjust height to floor
        AdjustHeightToFloor();

        // Enable movement
        EnableMovement();

        Debug.Log("Player stood up");
    }

    private void AdjustHeightToFloor()
    {
        if (characterController == null) return;

        // Calculate player's bottom point
        float bottomPoint = player.transform.position.y - originalHeight / 2f;

        // Raycast to find floor
        if (Physics.Raycast(player.transform.position + Vector3.up * 0.1f,
                           Vector3.down,
                           out RaycastHit hit,
                           2f))
        {
            float floorHeight = hit.point.y;
            float heightDifference = bottomPoint - floorHeight;
            player.transform.position -= new Vector3(0, heightDifference, 0);
        }
    }
}