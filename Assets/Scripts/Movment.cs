using UnityEngine;

public class Movment : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;

    [SerializeField] Rigidbody2D rigidbody;
    [SerializeField] Camera cam;

    Vector2 movement;
    Vector2 mousePos;

    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
    }

    private void FixedUpdate()
    {
        rigidbody.MovePosition(rigidbody.position + movement * moveSpeed * Time.fixedDeltaTime);

        Vector2 lookDirection = mousePos - rigidbody.position;
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90f;
        rigidbody.rotation = angle;
    }
}
