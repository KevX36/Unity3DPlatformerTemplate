using NUnit.Framework.Constraints;
using NUnit.Framework.Internal.Commands;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player input, animation, and feedback effects for character movement.
/// Works in conjunction with AdvancedMoveController to provide a complete character control system.
/// </summary>
[RequireComponent(typeof(AdvancedMoveController))]
public class PlayerController : MonoBehaviour
{
    public ThirdPersonCamera CameraFollower {get; private set;}
    private Animator characterAnimator;
    private AdvancedMoveController moveController;
    private Rigidbody rb;
    private DashController dashController;

    private Graple grapleController;
    // Movement state
    private Vector3 moveDirection;
    private Vector3 cameraAlignedForward;
    private Vector3 cameraAlignedRight;
    private Vector3 inputVector;
    // used to resume movement after graple
    private Vector3 movementStorage;
    // makes sure you can stop moving after using graple
    private bool ResumeMovment = false;
    //makes graple inactive while holding or pushing onjects
    private bool canGraple = true;
    



    private HealthController healthComponent;
    private PlayerInput playerInput;
    
    public bool JoinedThroughGameManager { get; set; } = false;
    public static List<PlayerController> players = new List<PlayerController>();

    //grapleing hook gameobject
    public GameObject GraplingHook;
    private void OnEnable()
    {
        if(moveController != null)
            moveController.enabled = true;
    }

    private void OnDisable()
    {
        if (moveController != null)
            {
                inputVector = Vector3.zero;
                moveDirection = Vector3.zero;
                rb.linearVelocity = Vector3.zero;
                moveController.ApplyMovement(Vector3.zero);
                moveController.UpdateMovement();
                moveController.enabled=false;
                UpdateVisualFeedback();
            }
    }

    /// <summary>
    /// Initialize components and verify required setup
    /// </summary>
    void Awake()
    {

        players.Add(this);
        // Ensure correct tag for player identification
        if(!gameObject.CompareTag("Player"))
            tag = "Player";

        TryGetComponent(out playerInput);
        TryGetComponent(out dashController);

        // Cache component references
        moveController = GetComponent<AdvancedMoveController>();
        rb = GetComponent<Rigidbody>();
        CameraFollower = GetComponentInChildren<ThirdPersonCamera>();
        characterAnimator = GetComponentInChildren<Animator>();
        healthComponent = GetComponent<HealthController>();

        grapleController = GraplingHook.GetComponent<Graple>();
        //makes sure graple is inactive otherwise controller will freeze
        GraplingHook.gameObject.SetActive(false);

        if (CameraFollower)
        {
            if (playerInput.camera == null) {
                //Debug.Log(actions["Jump"].GetBindingDisplayString());
                playerInput.camera = CameraFollower.GetComponent<Camera>();
            }
            CameraFollower.transform.SetParent(transform.parent);
            DontDestroyOnLoad(CameraFollower.gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        if (!JoinedThroughGameManager)
        {
            Destroy(gameObject);
            return;
        }
        CheckpointManager.TeleportPlayerToCheckpoint(gameObject);
    }

    /// <summary>
    /// Clean up camera follower on destruction
    /// </summary>
    void OnDestroy()
    {
        if (players.Contains(this))
            players.Remove(this);
        if (playerInput)
            Destroy(playerInput);
        if (CameraFollower)
            Destroy(CameraFollower.gameObject);
    }
    // changes if graple can be used
    public void ToggleCanGraple()
    {
        canGraple = !canGraple;
    }
    // shoots garpling hook
    void OnGraple()
    {
        
        //if added to all actions to insure garple is not active, stops actions while shooting the hook
        if (!GraplingHook.gameObject.activeSelf && canGraple)
        {
            if (!GameManager.Instance.IsShowingPauseMenu)
            {
                GraplingHook.gameObject.SetActive(true);
                
                grapleController.ShootGraple();
            }
        }
        



    }
    
    void OnMove(InputValue inputVal)
    {
        
        if (GameManager.Instance.IsShowingPauseMenu)
            inputVector = Vector3.zero;
        
        else
            inputVector = inputVal.Get<Vector2>();

        //stores movement input during graple to resume after
        movementStorage = inputVector;

    }
    /// <summary>
    /// Handle jump input from the input system
    /// </summary>
    void OnJump()
    {
        if (!GraplingHook.gameObject.activeSelf)
        {
            if (!GameManager.Instance.IsShowingPauseMenu)
                moveController.RequestJump();
        }
        
    }

    void OnPause()
    {

        GameManager.Instance.TogglePauseMenu();
        
        Debug.Log("tried to close game");
    }

    /// <summary>
    /// Handle dash input from the input system
    /// </summary>
    void OnDash()
    {
        if (!GraplingHook.gameObject.activeSelf)
        {
            if (!GameManager.Instance.IsShowingPauseMenu && dashController)
                dashController.TryStartDash(moveDirection);
        }
        
    }

    void OnCameraOrbit(InputValue inputVal)
    {
        CameraFollower.OrbitInput = inputVal.Get<float>();
    }

    /// <summary>
    /// Calculate movement direction based on camera orientation
    /// </summary>
    void Update()
    {
        // stops movement if garpling and resumes movement after garple without needing to press key again

        
        if (GraplingHook.gameObject.activeSelf)
        {
            ResumeMovment = true;
            inputVector = Vector3.zero;
        }
        else if (movementStorage != null && ResumeMovment)
        {
            
            Debug.Log("resumed movement");
            inputVector = movementStorage;
            ResumeMovment = false;
        }

        // Convert input to camera-relative movement direction
        Quaternion cameraRotation = Quaternion.Euler(0, CameraFollower.transform.eulerAngles.y, 0);
        cameraAlignedForward = cameraRotation * Vector3.forward;
        cameraAlignedRight = cameraRotation * Vector3.right;
        
        moveDirection = ((cameraAlignedForward * inputVector.y) + (cameraAlignedRight * inputVector.x)).normalized;
        
    }

    /// <summary>
    /// Handle physics-based movement and animation updates
    /// </summary>
    void FixedUpdate()
    {

        



        if (moveController.enabled)
        {

            moveController.ApplyMovement(moveDirection);
            moveController.UpdateMovement();
        }
        

        // Normal movement

        UpdateVisualFeedback();

        if (transform.position.y < -10f) {
            CheckpointManager.TeleportPlayerToCheckpoint(gameObject);
            if (CameraFollower)
                CameraFollower.transform.position = gameObject.transform.position;
        }
    }

    /// <summary>
    /// Update animator parameters and handle squash/stretch effects
    /// </summary>
    private void UpdateVisualFeedback()
    {
        if (!characterAnimator) return;

        // Update animator parameters
        characterAnimator.SetFloat(MovementController.AnimationID_DistanceToTarget, moveController.distanceToDestination);
        characterAnimator.SetBool(MovementController.AnimationID_IsGrounded, moveController.isGrounded);
        characterAnimator.SetFloat(MovementController.AnimationID_YVelocity, rb.linearVelocity.y);

        characterAnimator.SetBool("Grapling", GraplingHook.gameObject.activeSelf);
    }

} 