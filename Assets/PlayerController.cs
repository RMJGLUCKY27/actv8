using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float doubleJumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Referencias")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    [Header("Animaciones")]
    [SerializeField] private float groundCheckRadius = 0.2f;

    // Parámetros del Animator
    private const string SPEED = "Speed";
    private const string IS_GROUNDED = "IsGrounded";
    private const string IS_CROUCHING = "IsCrouching";
    private const string JUMP_TRIGGER = "Jump";
    private const string DOUBLE_JUMP_TRIGGER = "DoubleJump";
    private const string HIT_TRIGGER = "Hit";

    // Variables de estado
    private bool _isGrounded;
    private bool _canDoubleJump;
    private bool _isCrouching;
    private bool _isFacingRight = true;
    private float _horizontalInput;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        GetInput();
        HandleFlip();
    }

    void FixedUpdate()
    {
        Move();
        CheckGround();
        UpdateAnimator();
    }

    private void GetInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _isCrouching = Input.GetAxisRaw("Vertical") < 0;

        if (Input.GetButtonDown("Jump")) HandleJump();
    }

    private void HandleJump()
    {
        if (_isGrounded)
        {
            ExecuteJump(jumpForce);
            animator.SetTrigger(JUMP_TRIGGER);
            _canDoubleJump = true;
        }
        else if (_canDoubleJump)
        {
            ExecuteJump(doubleJumpForce);
            animator.SetTrigger(DOUBLE_JUMP_TRIGGER);
            _canDoubleJump = false;
        }
    }

    private void ExecuteJump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Reset Y
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse); // Física mejorada
    }

    private void Move()
    {
        if (!_isCrouching)
        {
            // Movimiento con linearVelocity
            rb.linearVelocity = new Vector2(_horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void CheckGround()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void UpdateAnimator()
    {
        animator.SetFloat(SPEED, Mathf.Abs(_horizontalInput));
        animator.SetBool(IS_GROUNDED, _isGrounded);
        animator.SetBool(IS_CROUCHING, _isCrouching);
    }

    private void HandleFlip()
    {
        if (_horizontalInput == 0) return;

        bool shouldFlip = (_horizontalInput < 0 && _isFacingRight) || 
                        (_horizontalInput > 0 && !_isFacingRight);
        
        if (shouldFlip) FlipCharacter();
    }

    private void FlipCharacter()
    {
        _isFacingRight = !_isFacingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    public void TakeDamage()
    {
        animator.SetTrigger(HIT_TRIGGER);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}