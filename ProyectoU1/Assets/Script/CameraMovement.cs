using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");   

        Vector3 movement = new Vector3(moveX, moveY, 0f).normalized;
        transform.position += movement * speed * Time.deltaTime;
    }
}
