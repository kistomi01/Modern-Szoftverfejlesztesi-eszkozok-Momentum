using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Vízszintes Mozgás")]
    public float moveSpeed = 12f; 
    private float moveInput;

    [Header("Ugrás Beállítások")]
    public float jumpForce = 11f;        
    public float fallMultiplier = 6f;      
    public float lowJumpMultiplier = 5f;   
    
    [Header("Sebesség Korlátozás")]
    public float maxFallSpeed = 25f;       // A maximum sebesség zuhanáskor

    [Header("Gravitáció Váltás")]
    private bool isUpsideDown = false;
    
    [Header("Talajérzékelés")]
    public Transform groundCheck;
    public float checkRadius = 0.3f;
    public LayerMask whatIsGround;
    
    [Header("Faldetektálás")]
    public float wallCheckDistance = 0.4f;  // Távolság a fal detektálásához
    private bool touchingWall = false;
    private float wallNormal = 0f;  // Az oldalfalak normálja
    
    [Header("Platform Kezelés")]
    private Collider2D lastGroundCollider = null;  // Az utolsó talaj amire léptünk
    private Rigidbody2D platformRigidbody = null;  // A platform rigidbodya
    
    [Header("Coyote Time (Ugrás Toleerancia)")]
    public float coyoteTime = 0.1f;  // Idő, amely alatt lehet ugrani a talaj elhagyása után
    private float coyoteCounter = 0f;
    
    [Header("Ugrás Bugyok Javítása")]
    private float jumpLockTimer = 0f;  // Időzítő az ugrás után
    private const float jumpLockDuration = 0.05f;  // Rövid idő, ahol nem lehet újra ugrálni
    private bool justJumped = false;  // Nyomon követjük, hogy épp ugrunk-e
    
    private Rigidbody2D rb;
    private bool jumpRequested;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Alapbeállítások a fizikai stabilitáshoz
        rb.freezeRotation = true;
        rb.gravityScale = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        // Bemenetek lekérése
        moveInput = Input.GetAxisRaw("Horizontal");

        // Ugrás figyelése
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W))
        {
            jumpRequested = true;
        }

        // Gravitáció váltás: E gomb vagy Bal egérgomb
        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
        {
            FlipGravity();
        }
    }

    void FixedUpdate()
    {
        // Jump lock timer csökkentése
        jumpLockTimer -= Time.fixedDeltaTime;
        if (jumpLockTimer <= 0)
        {
            justJumped = false;
        }

        // Talaj ellenőrzése (de csak akkor, ha nem vagyunk jumplock alatt)
        if (jumpLockTimer <= 0)
        {
            Collider2D groundCollider = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
            isGrounded = (groundCollider != null);
            
            if (isGrounded && groundCollider != lastGroundCollider)
            {
                lastGroundCollider = groundCollider;
                platformRigidbody = groundCollider.GetComponent<Rigidbody2D>();
            }
            else if (!isGrounded)
            {
                lastGroundCollider = null;
                platformRigidbody = null;
            }
        }

        // Fal detektálása (oldalirányú raycastok)
        DetectWalls();

        // Coyote Time kezelése: Ha grounded, resetelünk, ha nem, csökkentünk
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.fixedDeltaTime;
        }

        // Alap vízszintes mozgás
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Ugrás indítása (figyelembe véve az aktuális gravitációt és coyote time-ot)
        if (jumpRequested && coyoteCounter > 0)
        {
            float direction = isUpsideDown ? -1f : 1f;
            
            // A platform sebességének hozzáadása az ugráshoz (ha van mozgó platform)
            float platformVelocity = 0f;
            if (platformRigidbody != null)
            {
                platformVelocity = platformRigidbody.linearVelocity.y;
            }
            
            // AddForce használata az impulzus módban - ez hatékonyabban legyőzi a falütközéseket
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, platformVelocity);  // Reset Y sebesség, platform sebességgel kezdve
            rb.AddForce(Vector2.up * jumpForce * direction, ForceMode2D.Impulse);
            
            // Ha a fal mellett ugrunk, adjunk hozzá oldalirányú erőt a falból való kilépéshez
            if (touchingWall && wallNormal != 0f)
            {
                rb.AddForce(Vector2.right * -wallNormal * (jumpForce * 0.5f), ForceMode2D.Impulse);
            }
            
            jumpRequested = false;
            coyoteCounter = 0f;
            jumpLockTimer = jumpLockDuration;
            justJumped = true;
        }

        
        float currentGravity = isUpsideDown ? -1f : 1f;
        
        // Ha "esünk" (eltávolodunk a talajtól)
        if ((!isUpsideDown && rb.linearVelocity.y < 0) || (isUpsideDown && rb.linearVelocity.y > 0))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * currentGravity * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // Ha ugrunk, de elengedtük a gombot (rövid ugrás / low jump)
        else if (((!isUpsideDown && rb.linearVelocity.y > 0) || (isUpsideDown && rb.linearVelocity.y < 0)) 
                 && !(Input.GetButton("Jump") || Input.GetKey(KeyCode.W)))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * currentGravity * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        // Sebesség korlátozása (hogy ne gyorsuljon a végtelenségig)
        float clampedY = Mathf.Clamp(rb.linearVelocity.y, -maxFallSpeed, maxFallSpeed);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, clampedY);
    }

    void DetectWalls()
    {
        touchingWall = false;
        wallNormal = 0f;

        // Jobb oldal faldetektálása
        RaycastHit2D rightHit = Physics2D.Raycast(rb.position, Vector2.right, wallCheckDistance, whatIsGround);
        if (rightHit.collider != null)
        {
            touchingWall = true;
            wallNormal = 1f;  // Jobbra van a fal
        }

        // Bal oldal faldetektálása
        RaycastHit2D leftHit = Physics2D.Raycast(rb.position, Vector2.left, wallCheckDistance, whatIsGround);
        if (leftHit.collider != null)
        {
            touchingWall = true;
            wallNormal = -1f;  // Balra van a fal
        }
    }

    void FlipGravity()
    {
        isUpsideDown = !isUpsideDown;
        rb.gravityScale *= -1f;

        // LENDÜLET NULLÁZÁSA: Megszüntetjük a zuhanásból eredő sebességet váltáskor
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // Karakter vizuális megfordítása (Y tengelyen)
        Vector3 scaler = transform.localScale;
        scaler.y *= -1f;
        transform.localScale = scaler;
    }

    // Segítség a beállításhoz a Scene nézetben
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}