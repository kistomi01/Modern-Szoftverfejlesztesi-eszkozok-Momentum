using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RespawnLogicTest
{
    private GameObject playerObject;
    private PlayerController playerController;

    [SetUp]
    public void Setup()
    {
        playerObject = new GameObject("Player");
        playerController = playerObject.AddComponent<PlayerController>();
        playerObject.AddComponent<Rigidbody2D>();
        
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(playerObject.transform);
        playerController.groundCheck = groundCheck.transform;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(playerObject);
    }

    [UnityTest]
    public IEnumerator PlayerRespawnsAtCorrectPosition()
    {
        Vector3 respawnPos = new Vector3(10, 10, 0);
        playerController.damagePosition = respawnPos;
        
        playerController.RespawnAtDamagePoint();
        
        Assert.AreEqual(respawnPos, playerObject.transform.position);
        yield return null;
    }
}
