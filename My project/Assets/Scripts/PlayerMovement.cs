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
    public float checkRadius = 0.2f;
    public LayerMask whatIsGround;
    
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
        // Talaj ellenőrzése
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        // Alap vízszintes mozgás
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Ugrás indítása (figyelembe véve az aktuális gravitációt)
        if (jumpRequested && isGrounded)
        {
            float direction = isUpsideDown ? -1f : 1f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * direction);
            jumpRequested = false; 
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

        if (isGrounded) jumpRequested = false;
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