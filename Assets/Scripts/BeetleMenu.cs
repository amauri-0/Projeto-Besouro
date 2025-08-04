using UnityEngine;

public class BeetleMenu : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public float arriveThreshold = 0.05f;

    void Start()
    {
    }

    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            pointB.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, pointB.position) <= arriveThreshold)
            transform.position = pointA.position;
    }
}
