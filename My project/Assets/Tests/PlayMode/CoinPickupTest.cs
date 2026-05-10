using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CoinPickupTest
{
    private GameObject gameManagerObject;
    private GameManager gameManager;

    [SetUp]
    public void Setup()
    {
        gameManagerObject = new GameObject("GameManager");
        gameManager = gameManagerObject.AddComponent<GameManager>();
        GameManager.instance = gameManager;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(gameManagerObject);
    }

    [UnityTest]
    public IEnumerator CoinCountIncreasesOnPickup()
    {
        int initialCoins = gameManager.coinCount;
        gameManager.IncrementCoinCount();
        
        Assert.AreEqual(initialCoins + 1, gameManager.coinCount);
        yield return null;
    }
}
