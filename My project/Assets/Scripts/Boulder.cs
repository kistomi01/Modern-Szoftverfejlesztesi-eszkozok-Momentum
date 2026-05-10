using UnityEngine;

public class Boulder : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 200f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

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
            if (name.Contains("Giant") || name.Contains("Large") || transform.localScale.x > 2f)
            {
                GameManager.instance.Death();
            }
            else
            {
                PlayerController pc = collision.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.TakeDamage();
                }
            }
        }
    }

    public void ResetBoulder()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
    }
    }
