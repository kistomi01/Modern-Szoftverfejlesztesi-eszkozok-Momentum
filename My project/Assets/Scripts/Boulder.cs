using UnityEngine;

public class Boulder : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 200f;

    void Update()
    {
        // Continuous movement to the right
        transform.Translate(Vector2.right * speed * Time.deltaTime, Space.World);
        
        // Visual rotation
        transform.Rotate(Vector3.forward * -rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Call the existing death logic
            if (GameManager.instance != null)
            {
                GameManager.instance.Death();
            }
        }
    }
}
