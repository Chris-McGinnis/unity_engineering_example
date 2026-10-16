using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ProjectileFactory projectileFactory;
    [SerializeField] private Transform firePoint;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Camera aimCamera;
    [SerializeField, Min(0f)] private float aimRadius = 1f;
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0f)] private float jumpForce = 10f;
    [SerializeField, Min(0f)] private float groundCheckDistance = 0.08f;
    [SerializeField] private LayerMask groundLayers = ~0;

    private StateMachine stateMachine;
    private Rigidbody2D body;
    private Collider2D playerCollider;

    private float horizontalInput;
    private bool jumpPressed;
    private bool jumpRequested;
    private InputActionMap playerActionMap;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction aimAction;
    private InputAction shootAction;
    private Vector2 aimDirection = Vector2.right;

    public float HorizontalInput => horizontalInput;
    public bool IsGrounded { get; private set; }
    public bool JumpPressed => jumpPressed;
    
    public bool CanJump => IsGrounded && jumpRequested;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        if (body == null)
        {
            Debug.LogError("[Player] A Rigidbody2D component is required.", this);
            enabled = false;
            return;
        }

        body.constraints |= RigidbodyConstraints2D.FreezeRotation;
        IsGrounded = CheckGrounded();

        if (inputActions == null)
        {
            Debug.LogError("[Player] Assign an Input Actions asset.", this);
            enabled = false;
            return;
        }

        //Assign input actions
        playerActionMap = inputActions.FindActionMap("Player", true);
        moveAction = playerActionMap.FindAction("Move", true);
        jumpAction = playerActionMap.FindAction("Jump", true);
        aimAction = playerActionMap.FindAction("Aim", true);
        shootAction = playerActionMap.FindAction("Shoot", true);
    }

    private void Start()
    {
        // FSM: the Player owns the machine and gives it a starting state. States decide transitions themselves.
        stateMachine = new StateMachine();
        stateMachine.Initialize(new PlayerIdleState(stateMachine, this));
    }

    private void OnEnable()
    {
        PlayerHealthSystem.OnHealthChanged += HealthChanged;
        if (playerActionMap != null)
        {
            moveAction.performed += HandleMove;
            moveAction.canceled += HandleMove;
            jumpAction.performed += HandleJump;
            shootAction.performed += HandleShoot;
            playerActionMap.Enable();
        }
    }

    private void Update()
    {
        stateMachine?.Update();
        jumpPressed = false;
        UpdateAim();
    }

    private void FixedUpdate()
    {
        IsGrounded = CheckGrounded();

        Vector2 velocity = body.linearVelocity;
        velocity.x = horizontalInput * moveSpeed;
        if (CanJump)
        {
            velocity.y = jumpForce;
            IsGrounded = false;
        }

        body.linearVelocity = velocity;
        jumpRequested = false;
    }

    private void OnDisable()
    {
        PlayerHealthSystem.OnHealthChanged -= HealthChanged;
        if (playerActionMap != null)
        {
            moveAction.performed -= HandleMove;
            moveAction.canceled -= HandleMove;
            jumpAction.performed -= HandleJump;
            shootAction.performed -= HandleShoot;
            playerActionMap.Disable();
        }

        horizontalInput = 0f;
        jumpPressed = false;
        jumpRequested = false;
    }

    private void HandleMove(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<float>();
    }

    private void HandleJump(InputAction.CallbackContext context)
    {
        jumpPressed = true;
        if (CanJump)
        {
            stateMachine.ChangeState(new PlayerJumpState(stateMachine, this));
        }
    }

    private void HandleShoot(InputAction.CallbackContext context)
    {
        UpdateAim();
        ShootProjectile();
    }

    private void UpdateAim()
    {
        Camera cam = aimCamera != null ? aimCamera : Camera.main;
        if (cam == null)
            return;

        Vector2 screenPosition = aimAction.ReadValue<Vector2>();
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y,
            Mathf.Abs(cam.transform.position.z - transform.position.z)));
        Vector2 toMouse = (Vector2)world - (Vector2)transform.position;
        if (toMouse.sqrMagnitude > 0.0001f)
            aimDirection = toMouse.normalized;

        if (firePoint != null)
        {
            firePoint.position = (Vector2)transform.position + aimDirection * aimRadius;
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            firePoint.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    public void RequestJump()
    {
        if (IsGrounded)
        {
            jumpRequested = true;
        }
    }

    private bool CheckGrounded()
    {
        if (playerCollider == null)
            return false;

        Bounds bounds = playerCollider.bounds;
        Vector2 checkCenter = new Vector2(bounds.center.x, bounds.min.y - groundCheckDistance * 0.5f);
        Vector2 checkSize = new Vector2(bounds.size.x * 0.8f, groundCheckDistance);
        return Physics2D.OverlapBox(checkCenter, checkSize, 0f, groundLayers) != null;
    }

    private void HealthChanged(float currentHealth, float maxHealth)
    {
        if (spriteRenderer != null && maxHealth > 0f)
            spriteRenderer.color = Color.Lerp(Color.red, Color.white, currentHealth / maxHealth);
    }

    public void ShootProjectile()
    {
        if (projectileFactory == null)
        {
            Debug.LogError("[Player] Assign a projectile factory before shooting.", this);
            return;
        }

        Transform spawnTransform = firePoint != null ? firePoint : transform;
        Projectile projectile = projectileFactory.CreateProjectile(
            spawnTransform.position,
            Quaternion.Euler(0f, 0f, Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg),
            aimDirection);

        if (projectile != null && playerCollider != null &&
            projectile.TryGetComponent(out Collider2D projectileCollider))
        {
            Physics2D.IgnoreCollision(projectileCollider, playerCollider);
        }
    }
}
