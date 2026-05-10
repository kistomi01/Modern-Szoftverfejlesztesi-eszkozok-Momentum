using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI References")]
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TMP_Text leveCompletePanelTitle;
    [SerializeField] private TMP_Text levelCompleteCoins;

    [Header("Player Reference")]
    [SerializeField] private PlayerController playerController;

    private int coinCount = 0;
    private int gemCount = 0;
    private int totalCoins = 0;
    private bool isGameOver = false;
    private bool isLevelComplete = false; // Új: Megakadályozza a többszöri befejezést

    private void Awake()
    {
        // Singleton minta megerősítése
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        isGameOver = false;
        isLevelComplete = false;
        coinCount = 0;

        // Kezdő sötétítés feloldása
        if (UIManager.instance != null)
        {
            UIManager.instance.fadeFromBlack = true;
        }

        FindTotalPickups();
        UpdateGUI();
    }

    public void IncrementCoinCount()
    {
        coinCount++;
        UpdateGUI();
    }

    public void IncrementGemCount()
    {
        gemCount++;
    }

    private void UpdateGUI()
    {
        if (coinText != null)
        {
            coinText.text = coinCount.ToString() + " / " + totalCoins.ToString();
        }
    }

    public void FindTotalPickups()
    {
        totalCoins = 0;
        pickup[] pickups = GameObject.FindObjectsOfType<pickup>();

        foreach (pickup pickupObject in pickups)
        {
            if (pickupObject.pt == pickup.pickupType.coin)
            {
                totalCoins += 1;
            }
        }
    }

    // --- PÁLYA VÉGE LOGIKA ---

    public void LevelComplete()
    {
        // Ha már folyamatban van a befejezés vagy a halál, ne fusson le újra
        if (isLevelComplete || isGameOver) return;
        
        isLevelComplete = true;

        // Játékos megállítása
        if (playerController != null)
        {
            playerController.isPaused = true;
            Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic; // Megállítja a fizikát, hogy ne triggereljen többször
            }
        }

        // Képernyő sötétítése
        if (UIManager.instance != null)
        {
            UIManager.instance.fadeToBlack = true;
        }

        StartCoroutine(LevelCompleteCoroutine());
    }

    private IEnumerator LevelCompleteCoroutine()
    {
        // Várunk, amíg a sötétítés tart (kb 1-2 másodperc)
        yield return new WaitForSeconds(1.5f); 

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            leveCompletePanelTitle.text = "LEVEL COMPLETE";
            levelCompleteCoins.text = "COINS COLLECTED: " + coinCount.ToString() + " / " + totalCoins.ToString();
        }
    }

    public void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("Betöltés: " + nextSceneIndex + " sorszámú pálya");
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("Nincs több pálya. Irány a menü.");
            SceneManager.LoadScene(0);
        }
    }

    // --- HALÁL LOGIKA ---

    public void Death()
    {
        if (!isGameOver && !isLevelComplete)
        {
            isGameOver = true;

            if (UIManager.instance != null)
            {
                UIManager.instance.DisableMobileControls();
                UIManager.instance.fadeToBlack = true;
            }

            if (playerController != null)
            {
                playerController.gameObject.SetActive(false);
            }

            StartCoroutine(DeathCoroutine());
            Debug.Log("Player meghalt.");
        }
    }

    public IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}