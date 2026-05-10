using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

// Meghatározzuk a vezérlési módokat, hogy ne legyen CS0246 hiba
public enum Controls { mobile, pc }

public class PlayerController : MonoBehaviour
{
    [Header("Mozgás Beállítások")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float doubleJumpForce = 8f;
    public Controls controlmode;

    [Header("Talajérzékelés")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    private bool isGroundedBool = false;
    private bool canDoubleJump = false;

    [Header("Effektek & Animáció")]
    public Animator playeranim;
    public ParticleSystem footsteps;
    private ParticleSystem.EmissionModule footEmissions;
    public ParticleSystem ImpactEffect;
    private bool wasonGround;

    [Header("Harcrendszer & Státusz")]
    public float fireRate = 0.5f; 
    private float nextFireTime = 0f; 
    private Vector3 damagePosition;
    public bool isInvulnerable = false;
    public float invulnerabilityDuration = 1.5f;
    private Vector3 lastGroundedPosition;

    private Rigidbody2D rb;
    private float moveX;
    public bool isPaused = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (footsteps != null) footEmissions = footsteps.emission;

        if (controlmode == Controls.mobile && UIManager.instance != null)
        {
            UIManager.instance.EnableMobileControls();
        }

        damagePosition = transform.position;
        lastGroundedPosition = transform.position;
    }

    private void Update()
    {
        if (isPaused)
        {
            moveX = 0;
            return;
        }

        isGroundedBool = IsGrounded();

        if (isGroundedBool)
        {
            lastGroundedPosition = transform.position;
            canDoubleJump = true; 

            if (controlmode == Controls.pc)
            {
                moveX = Input.GetAxisRaw("Horizontal");
            }

            if (Input.GetButtonDown("Jump"))
            {
                Jump(jumpForce);
            }
        }
        else
        {
            // JAVÍTÁS: Csak akkor engedünk dupla ugrást, ha a gombot MOST nyomták meg és még van rá lehetőség
            if (canDoubleJump && Input.GetButtonDown("Jump"))
            {
                Jump(doubleJumpForce);
                canDoubleJump = false; 
            }
            
            if (controlmode == Controls.pc)
            {
                moveX = Input.GetAxisRaw("Horizontal");
            }
        }

        // Lövés kezelése (ha van ilyen funkciód)
        if (controlmode == Controls.pc && Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }

        SetAnimations();

        if (moveX != 0)
        {
            FlipSprite(moveX);
        }

        // Földet érés effekt
        if (!wasonGround && isGroundedBool && ImpactEffect != null)
        {
            ImpactEffect.gameObject.SetActive(true);
            ImpactEffect.Stop();
            ImpactEffect.transform.position = groundCheck.position;
            ImpactEffect.Play();
        }

        wasonGround = isGroundedBool;
    }

    public void SetAnimations()
    {
        if (playeranim == null) return;

        bool isRunning = Mathf.Abs(moveX) > 0.1f && isGroundedBool && !isPaused;
        playeranim.SetBool("run", isRunning);
        
        if (footsteps != null)
            footEmissions.rateOverTime = isRunning ? 35f : 0f;

        playeranim.SetBool("isGrounded", isGroundedBool);
    }

    private void FlipSprite(float direction)
    {
        if (isPaused) return;
        if (direction > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (direction < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    private void FixedUpdate()
    {
        if (isPaused)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
    }

    private void Jump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        if (playeranim != null) playeranim.SetTrigger("jump");
    }

    private bool IsGrounded()
    {
        // Megnöveltük a sugarat 0.3-ra a biztosabb érzékelés érdekében.
        // Kizárjuk a játékos saját colliderét és minden gyermekét, hogy ne legyen végtelen ugrás.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, 0.3f, groundLayer);
        foreach (var col in colliders)
        {
            if (col.transform.root != transform.root && !col.isTrigger) 
            {
                return true;
            }
        }
        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isInvulnerable) return;

        if (collision.gameObject.CompareTag("killzone"))
        {
            damagePosition = lastGroundedPosition;
            HealthManager.instance.HurtPlayer();
            return;
        }

        // Tüskék és egyedi csempék érzékelése
        Tilemap tilemap = collision.gameObject.GetComponent<Tilemap>();
        if (tilemap != null)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                Vector3Int tilePos = tilemap.WorldToCell(contact.point - contact.normal * 0.1f);
                TileBase tile = tilemap.GetTile(tilePos);
                if (tile != null && (tile.name.Contains("Spike") || tile.name.Contains("Tree")))
                {
                    damagePosition = transform.position;
                    HealthManager.instance.HurtPlayer();
                    break;
                }
            }
        }
    }

    public void TakeDamage()
    {
        if (isInvulnerable) return;
        damagePosition = transform.position;
        HealthManager.instance.HurtPlayer();
    }

    // JAVÍTÁS: A Respawn most már teljesen alaphelyzetbe állítja a mozgást
    public void RespawnAtDamagePoint()
    {
        if (rb != null) 
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        transform.position = damagePosition;

        // Töröljük a beragadt ugrásokat
        canDoubleJump = false; 
        moveX = 0;
        wasonGround = true;

        StopAllCoroutines(); 
        StartCoroutine(InvulnerabilityCoroutine());
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        
        float elapsed = 0f;
        while (elapsed < invulnerabilityDuration)
        {
            if (sr != null) sr.enabled = !sr.enabled; // Villogás
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (sr != null) sr.enabled = true;
        isInvulnerable = false;
    }

    private void Shoot()
    {
        Debug.Log("Lövés!");
    }
}