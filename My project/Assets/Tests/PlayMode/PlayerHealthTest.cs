using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerHealthTest
{
    private GameObject healthManagerObject;
    private GameObject playerObject;
    private HealthManager healthManager;
    private PlayerController playerController;

    [SetUp]
    public void Setup()
    {
        healthManagerObject = new GameObject("HealthManager");
        healthManager = healthManagerObject.AddComponent<HealthManager>();
        HealthManager.instance = healthManager;
        
        playerObject = new GameObject("Player");
        playerObject.tag = "Player";
        playerController = playerObject.AddComponent<PlayerController>();
        playerObject.AddComponent<Rigidbody2D>();
        
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(playerObject.transform);
        playerController.groundCheck = groundCheck.transform;

        // HealthManager normally gets player in Start, but we can set it up manually or wait for Start
        // We'll set the currentHealth manually for the test
        healthManager.currentHealth = 3;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(healthManagerObject);
        Object.DestroyImmediate(playerObject);
    }

    [UnityTest]
    public IEnumerator PlayerHealthDecreasesOnHurt()
    {
        int initialHealth = healthManager.currentHealth;
        healthManager.HurtPlayer();
        
        Assert.AreEqual(initialHealth - 1, healthManager.currentHealth);
        yield return null;
    }

    [UnityTest]
    public IEnumerator PlayerDoesNotTakeDamageWhenInvulnerable()
    {
        playerController.isInvulnerable = true;
        
        // We need to make sure HealthManager knows about this player script
        // In HealthManager.Start() it does GameObject.FindObjectOfType<PlayerController>()
        // Since we created it in Setup, Start should have run if we wait a frame or call it manually
        // But HealthManager.HurtPlayer uses 'playerScript'. Let's use reflection to set it or just wait.
        
        var field = typeof(HealthManager).GetField("playerScript", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(healthManager, playerController);

        int initialHealth = healthManager.currentHealth;
        healthManager.HurtPlayer();
        
        Assert.AreEqual(initialHealth, healthManager.currentHealth);
        yield return null;
    }
}
