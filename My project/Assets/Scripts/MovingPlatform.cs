using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector2 direction;
    public float distance;
    public float speed;
    
    private Vector2 startPos;
    private int factor = 1;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float movement = Mathf.PingPong(Time.time * speed, distance);
        transform.position = startPos + direction.normalized * movement;
    }
}
