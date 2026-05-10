using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return;

        if (collision.CompareTag("Player"))
        {
            isTriggered = true;
            
            var pc = collision.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.isPaused = true;
                var rb = pc.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
            }

            // Azonnal hívjuk a Managert, nem várunk itt helyben
            if (GameManager.instance != null)
            {
                GameManager.instance.LevelComplete();
            }
        }
    }
}