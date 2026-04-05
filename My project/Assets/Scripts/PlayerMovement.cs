using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Vízszintes Mozgás")]
    public float moveSpeed = 12f;
    private float moveInput;

    [Header("Ugrás Beállítások")]
    public float jumpForce = 18f;
    public float fallMultiplier = 5.5f;
    public float lowJumpMultiplier = 12f; 
    public float maxFallSpeed = 30f;
    public float maxJumpHoldTime = 0.16f;

    [Header("Talajérzékelés")]
    public Transform groundCheck;
    public float checkRadius = 0.3f;
    public LayerMask whatIsGround;
    public float coyoteTime = 0.1f;

    [Header("Gravitáció Mérő (UI)")]
    public float maxGravityTime = 5f;        
    public float switchPenalty = 1f;         
    public float rechargeRate = 1.5f;        
    public float pixelsPerSecond = 40f;      // 1 másodperc hány pixel széles legyen a képernyőn?
    public Slider gravityBar;                
    public Image barFillImage;               
    public Color fullColor = Color.cyan;     
    public Color emptyColor = Color.red;    
    
    private float currentGravityTime;
    private Rigidbody2D rb;
    private bool isUpsideDown = false;
    private bool jumpRequested;
    private bool isGrounded;
    private float coyoteCounter;
    private float jumpHoldCounter;
    private Rigidbody2D platformRb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        currentGravityTime = maxGravityTime;

        if (gravityBar != null)
        {
            // 1. Beállítjuk a Slider belső értékeit
            gravityBar.minValue = 0;
            gravityBar.maxValue = maxGravityTime;
            gravityBar.value = maxGravityTime;

            // 2. DINAMIKUS MÉRETEZÉS: Átméretezzük a Slider-t a max idő alapján
            // Így ha kevesebb az idő, rövidebb lesz a csík, de teljesen teli
            RectTransform rt = gravityBar.GetComponent<RectTransform>();
            if (rt != null)
            {
                float newWidth = maxGravityTime * pixelsPerSecond;
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
            }
        }
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W))
            jumpRequested = true;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
        {
            if (!isUpsideDown && currentGravityTime > switchPenalty)
            {
                currentGravityTime -= switchPenalty;
                FlipGravity();
            }
            else if (isUpsideDown)
            {
                FlipGravity();
            }
        }

        UpdateGravityMeter();
    }

    private void UpdateGravityMeter()
    {
        if (isUpsideDown)
        {
            currentGravityTime -= Time.deltaTime;

            if (currentGravityTime <= 0)
            {
                currentGravityTime = 0;
                FlipGravity();
            }
        }
        else if (isGrounded)
        {
            currentGravityTime = Mathf.MoveTowards(currentGravityTime, maxGravityTime, rechargeRate * Time.deltaTime);
        }

        if (gravityBar != null)
        {
            gravityBar.value = currentGravityTime;
            
            if (barFillImage != null)
            {
                float ratio = currentGravityTime / maxGravityTime;
                barFillImage.color = Color.Lerp(emptyColor, fullColor, ratio);
            }
        }
    }

    void FixedUpdate()
    {
        HandleGroundCheck();
        HandleHorizontalMovement();
        HandleJump();
        ApplyCustomGravity();
    }

    private void HandleGroundCheck()
    {
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        isGrounded = hit != null;

        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
            platformRb = hit.GetComponent<Rigidbody2D>();
        }
        else
        {
            coyoteCounter -= Time.fixedDeltaTime;
            platformRb = null;
        }
    }

    private void HandleHorizontalMovement()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    private void HandleJump()
    {
        if (jumpRequested && coyoteCounter > 0)
        {
            float gravityDir = isUpsideDown ? -1f : 1f;
            float pVelY = platformRb != null ? platformRb.linearVelocity.y : 0f;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, pVelY);
            rb.AddForce(Vector2.up * jumpForce * gravityDir, ForceMode2D.Impulse);

            coyoteCounter = 0;
            jumpHoldCounter = maxJumpHoldTime;
            jumpRequested = false;
        }
        
        if (!Input.GetButton("Jump") && !Input.GetKey(KeyCode.W))
            jumpRequested = false;
    }

    private void ApplyCustomGravity()
    {
        float gravityDir = isUpsideDown ? -1f : 1f;
        float verticalVel = rb.linearVelocity.y * gravityDir;
        bool jumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.W);

        if (jumpHeld && jumpHoldCounter > 0f)
            jumpHoldCounter -= Time.fixedDeltaTime;

        if (verticalVel < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * gravityDir * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (verticalVel > 0 && (!jumpHeld || jumpHoldCounter <= 0f))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * gravityDir * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        float clampedY = Mathf.Clamp(rb.linearVelocity.y, -maxFallSpeed, maxFallSpeed);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, clampedY);
    }

    private void FlipGravity()
    {
        isUpsideDown = !isUpsideDown;
        rb.gravityScale *= -1f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        transform.localScale = new Vector3(transform.localScale.x, -transform.localScale.y, transform.localScale.z);
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}