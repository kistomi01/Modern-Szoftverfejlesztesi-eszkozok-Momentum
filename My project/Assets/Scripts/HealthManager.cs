using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public static HealthManager instance;
    public GameObject damageEffect;

    private int MaxHealth = 3;
    public int currentHealth;

    [SerializeField] private UnityEngine.UI.Image[] hearts;
    [SerializeField] private Sprite FullHeartSprite;
    [SerializeField] private Sprite EmptyHeartSprite;

    private PlayerController playerScript;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        playerScript = GameObject.FindObjectOfType<PlayerController>();
        currentHealth = MaxHealth;
        DisplayHearts();
    }

    public void HurtPlayer()
    {
        // JAVÍTÁS: Ha a játékos sérthetetlen, nem vesszük le az életét
        if (playerScript != null && playerScript.isInvulnerable) return;

        if (currentHealth <= 0) return;

        currentHealth--;
        DisplayHearts();
        
        if (damageEffect != null && playerScript != null)
            Instantiate(damageEffect, playerScript.transform.position, Quaternion.identity);

        if (currentHealth <= 0)
        {
            if (GameManager.instance != null) GameManager.instance.Death();
        }
        else
        {
            if (GameManager.instance != null) GameManager.instance.DamageRespawn();
        }
    }

    public void DisplayHearts()
    {
        if (hearts == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].enabled = true;
                if (FullHeartSprite != null) hearts[i].sprite = FullHeartSprite;
            }
            else
            {
                if (EmptyHeartSprite != null)
                {
                    hearts[i].sprite = EmptyHeartSprite;
                    hearts[i].enabled = true;
                }
                else
                {
                    hearts[i].enabled = false;
                }
            }
        }
    }
}