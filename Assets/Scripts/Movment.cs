using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rigidbody;
    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private GameObject followCameraPrefab;
    [SerializeField] private Transform weaponObject;
    [SerializeField] private Transform playerVisual;
    private Camera playerCamera;
    private CinemachineCamera followCamera;

    Vector2 movement;
    Vector2 mousePos;

    [Header("Dash Variables")]
    [SerializeField] private bool   allowDashInput   = true;
    /*not implimented*/ //[SerializeField] private bool enemyCollition = false;
    /*not implimented*/ //[SerializeField] private bool playerCollition = false;
    /*not implimented*/ //[SerializeField] private bool wallCollition = true; 
    /*not implimented*/ //[SerializeField] private bool invincible = true; 
    [SerializeField] private float  dashDistance     = 5f;     // (unity units)
    [SerializeField] private float  dashTime         = 0.2f;   // (seconds)
    [SerializeField] private float  dashChargeUpTime = 0.0f;   // (seconds)
    [SerializeField] private float  dashCooldownTime = 1f;     // (seconds)
    [SerializeField] private bool   dashOnCooldown   = false;  // (seconds)    
    private Wrapper<bool> dashing = new Wrapper<bool>(false);
    //private GeneralizedDash2D dash;

    private void Start()
    {
        GameObject cameraInstance = Instantiate(cameraPrefab, transform.position, Quaternion.identity);
        GameObject followCameraInstance = Instantiate(followCameraPrefab, transform.position, Quaternion.identity);
        playerCamera = cameraInstance.GetComponent<Camera>();
        followCamera = followCameraInstance.GetComponent<CinemachineCamera>();
        if (followCamera != null) {
            followCamera.Follow = this.transform;
            followCamera.Lens.OrthographicSize = 4.5f;
        }
        playerCamera.enabled = true;
        followCamera.enabled = true;
    }

    void Update()
    {
        if (dashing.value) { return; } // Do not allow movement while dashing

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

        // Dash input
        if (allowDashInput && Input.GetKeyDown(KeyCode.Space) && !dashOnCooldown)
        {
            // Update mouse position in case it moved
            mousePos = (Vector2)playerCamera.ScreenToWorldPoint(Input.mousePosition);

            /*// Calculate the dash direction as a **normalized vector** (ensuring constant speed).
            Vector2 dashDirection = (dashTarget - rb.position).normalized;
            */

            StartCoroutine(GeneralizedDash2D.Dash2D(dashDistance, dashTime, dashChargeUpTime, movement, gameObject, dashing));
            if (dashing.value) { StartCoroutine(DashCooldown()); }
            // hide gun logic here if needed
        }
    }

    private void FixedUpdate()
    {
        if (dashing.value) { return; } // Do not allow movement while dashing

        // Move the player
        rigidbody.MovePosition(rigidbody.position + movement * moveSpeed * Time.fixedDeltaTime);

        // Rotate the weapon to face the mouse
        Vector2 lookDirection = mousePos - (Vector2)weaponObject.position;
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        weaponObject.rotation = Quaternion.Euler(0, 0, angle);
    }

    private IEnumerator DashCooldown()
    {
        dashOnCooldown = true;
        yield return new WaitForSeconds(dashCooldownTime);
        dashOnCooldown = false;
    }
}