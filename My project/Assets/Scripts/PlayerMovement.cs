using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Vízszintes Mozgás")]
    public float moveSpeed = 10f;
    private float moveInput;

    [Header("Ugrás Beállítások")]
    public float jumpForce = 14f;
    public float fallMultiplier = 4f;      // Gyorsabb esés, kevésbé lebegős
    public float lowJumpMultiplier = 3f;   // Gyorsabb rövid ugrás, ha elengeded
    
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
        
        // Fontos: ne dőljön el a karakter a sarkaival ütközve
        rb.freezeRotation = true;
        
        // Kezdésnek állítsuk a gravitációt alapértelmezettre, a kód módosítja majd
        rb.gravityScale = 1f;
    }

    void Update()
    {
        // Bemenet lekérése (A/D vagy Nyilak)
        moveInput = Input.GetAxisRaw("Horizontal");

        // Ugrás figyelése (Space vagy W)
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W))
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        // Talaj ellenőrzése
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        // Alap vízszintes mozgás
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Ugrás indítása
        if (jumpRequested && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpRequested = false; 
        }

        // --- OKOS GRAVITÁCIÓ (Itt szűnik meg a lebegés) ---
        
        if (rb.linearVelocity.y < 0) // Ha esünk lefelé
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !(Input.GetButton("Jump") || Input.GetKey(KeyCode.W))) // Ha ugrunk, de elengedtük a gombot
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        // Ha a földön vagyunk, ne gyűjtsük az ugrási kérelmet
        if (isGrounded) jumpRequested = false;
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