using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rigidbody;
    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private Transform weaponObject;
    [SerializeField] private Transform playerVisual;
    private Camera playerCamera;

    Vector2 movement;
    Vector2 mousePos;

    private void Start()
    {
        GameObject cameraInstance = Instantiate(cameraPrefab, transform.position, Quaternion.identity);
        playerCamera = cameraInstance.GetComponent<Camera>();
        CameraController controller = cameraInstance.GetComponent<CameraController>();
        if (controller != null)
            controller.SetTarget(this.gameObject);

        playerCamera.enabled = true;
    }

    void Update()
    {
        // Movement input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Get mouse position in world space
        mousePos = playerCamera.ScreenToWorldPoint(Input.mousePosition);

        // Flip player visual if necessary
        if (mousePos.x > transform.position.x)
            playerVisual.localScale = new Vector3(1, 1, 1); // Normal scale (facing right)
        else
            playerVisual.localScale = new Vector3(-1, 1, 1); // Flipped scale (facing left)
    }

    private void FixedUpdate()
    {
        // Move the player
        rigidbody.MovePosition(rigidbody.position + movement * moveSpeed * Time.fixedDeltaTime);

        // Rotate the weapon to face the mouse
        Vector2 lookDirection = mousePos - (Vector2)weaponObject.position;
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        weaponObject.rotation = Quaternion.Euler(0, 0, angle);
    }
}
