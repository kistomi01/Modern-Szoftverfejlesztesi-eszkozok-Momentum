using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    [Header("Mozgás Beállítások")]
    public float moveSpeed = 4f;
    private bool movingRight = true;

    [Header("Érzékelés")]
    public Transform wallCheck;      
    public float checkRadius = 0.2f;
    public LayerMask whatIsGround;
    public float edgeCheckDistance = 0.5f;
    public float turnCooldown = 0.2f;

    private Rigidbody2D rb;
    private float nextTurnTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Fontos: ne dőljön el a karakter
        rb.freezeRotation = true; 
        
        // Ütközés detektálás legyen folyamatos
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void FixedUpdate()
    {
        // Mozgás
        rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);

        // 1. Szakadék figyelése (lefelé a wallCheck pontból)
        RaycastHit2D groundHit = Physics2D.Raycast(wallCheck.position, Vector2.down, edgeCheckDistance, whatIsGround);
        bool isGroundAhead = groundHit.collider != null;

        // 2. Fal figyelése (vízszintesen a mozgás irányába)
        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, direction, checkRadius * 2, whatIsGround);

        // Ha nincs talaj VAGY falnak ütközik -> Megfordulás
        if (( !isGroundAhead || wallHit.collider != null ) && Time.time >= nextTurnTime)
        {
            Flip();
        }
    }

    void Flip()
    {
        movingRight = !movingRight;
        nextTurnTime = Time.time + turnCooldown;
        // Megfordítjuk a sprite-ot vízszintesen
        transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
    }

    
    private void OnDrawGizmos()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(wallCheck.position, checkRadius);
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.down * edgeCheckDistance);
            
            Vector2 direction = movingRight ? Vector2.right : Vector2.left;
            Gizmos.DrawLine(wallCheck.position, (Vector2)wallCheck.position + direction * (checkRadius * 2));
        }
    }
}