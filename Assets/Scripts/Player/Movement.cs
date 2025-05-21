using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rigidbody;
    [SerializeField] private Transform weaponObject;
    [SerializeField] private Transform playerVisual;
    private Camera playerCamera;

    Vector2 movement;
    Vector2 mousePos;

    [Header("Dash Variables")]
    [SerializeField] private bool allowDashInput = true;
    /*not implimented*/ //[SerializeField] private bool enemyCollition = false;
    /*not implimented*/ //[SerializeField] private bool playerCollition = false;
    /*not implimented*/ //[SerializeField] private bool wallCollition = true; 
    /*not implimented*/ //[SerializeField] private bool invincible = true; 
    [SerializeField] private float dashDistance = 5f;     // (unity units)
    [SerializeField] private float dashTime = 0.2f;   // (seconds)
    [SerializeField] private float dashChargeUpTime = 0.0f;   // (seconds)
    [SerializeField] private float dashCooldownTime = 1f;     // (seconds)
    [SerializeField] private bool dashOnCooldown = false;  // (seconds)    
    private Wrapper<bool> dashing = new Wrapper<bool>(false);

    [Header("Audio")]
    [SerializeField] private string walkingSfxName = "Walk_Grass";
    private AudioSource _walkingSfxSource;

    [Header("Nerfed Movement")]
    [SerializeField, Tooltip("Speed gets multiplied with this during actions like reloading.")] 
    float speedMultiplier = 0.5f;
    bool nerfMovement = false;
    float baseMoveSpeed;
    private void Start()
    {
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        nerfMovement = false;
        baseMoveSpeed = moveSpeed;
    }

    void Update()
    {
        HandleNerfedMovement();
        if (dashing.value)
        {
            if (_walkingSfxSource != null)
            {
                if (AudioPlayer.Current != null)
                {
                    AudioPlayer.Current.StopLoopingSfx(_walkingSfxSource);
                }
                _walkingSfxSource = null;
            }
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        mousePos = playerCamera.ScreenToWorldPoint(Input.mousePosition);

        playerVisual.localScale = new Vector3((movement.x > 0) ? 1 : (movement.x < 0) ? -1 : playerVisual.localScale.x, 1, 1);

        bool isActuallyMoving = movement.sqrMagnitude > 0.01f;

        if (isActuallyMoving)
        {
            // If not already playing a walking sound AND AudioPlayer is available AND sfx name is set
            if (_walkingSfxSource == null && AudioPlayer.Current != null && !string.IsNullOrEmpty(walkingSfxName))
            {
                // Attempt to play the sound, parent it to this transform for 3D audio positioning
                _walkingSfxSource = AudioPlayer.Current.PlayLoopingSfx(walkingSfxName, transform.position, 1.0f, transform, true);
                if (_walkingSfxSource == null)
                {
                    // This means PlayLoopingSfx returned null, likely because the clip name was not found.
                    Debug.LogWarning($"Movement: AudioPlayer failed to play SFX '{walkingSfxName}'. Clip might be missing or name incorrect in AudioPlayer sfxClips.");
                }
            }
            // If AudioPlayer became null while we thought a sound was playing, clear our reference.
            else if (_walkingSfxSource != null && AudioPlayer.Current == null)
            {
                // AudioPlayer is gone, so we can't be "playing" its sound.
                _walkingSfxSource = null;
            }
        }
        else // Not moving
        {
            // If a walking sound is supposed to be playing
            if (_walkingSfxSource != null)
            {
                // Try to stop it via AudioPlayer if available
                if (AudioPlayer.Current != null)
                {
                    AudioPlayer.Current.StopLoopingSfx(_walkingSfxSource);
                }
                // In any case, we are no longer "playing" this sound from Movement's perspective
                _walkingSfxSource = null;
            }
        }


        if (!nerfMovement && allowDashInput && Input.GetKeyDown(KeyCode.Space) && !dashOnCooldown)
        {
            if (_walkingSfxSource != null)
            {
                if (AudioPlayer.Current != null)
                {
                    AudioPlayer.Current.StopLoopingSfx(_walkingSfxSource);
                }
                _walkingSfxSource = null;
            }

            mousePos = (Vector2)playerCamera.ScreenToWorldPoint(Input.mousePosition);

            StartCoroutine(GeneralizedDash2D.Dash2D(dashDistance, dashTime, dashChargeUpTime, movement, gameObject, dashing));
            if (dashing.value) { 
                StartCoroutine(DashCooldown()); 
                AudioPlayer.Current.PlaySfxAtPoint("DodgeSwoosh", transform.position); 
            }
        }
    }

    private void FixedUpdate()
    {
        if (dashing.value) { return; }

        rigidbody.MovePosition(rigidbody.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private IEnumerator DashCooldown()
    {
        dashOnCooldown = true;
        yield return new WaitForSeconds(dashCooldownTime);
        dashOnCooldown = false;
    }

    public void EnableNerfedMovement() { nerfMovement = true; }
    public void DisableNerfedMovement() { nerfMovement = false; }
    private void HandleNerfedMovement()
    {
        if (nerfMovement)
        {
            moveSpeed = baseMoveSpeed * speedMultiplier;
        }
        else
        {
            moveSpeed = baseMoveSpeed;
        }
    }
}