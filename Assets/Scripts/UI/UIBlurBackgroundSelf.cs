using UnityEngine;
using UnityEngine.UI; // Required for Image/RawImage

[RequireComponent(typeof(MaskableGraphic))] // Works with Image or RawImage
public class UIBlurBackgroundSelf : MonoBehaviour
{
    [Header("Assignments")]
    [Tooltip("The Material that uses your blur shader (e.g., your Gaussian blur shader).")]
    [SerializeField] private Material blurMaterial;
    [Tooltip("Optional: If not assigned, will try to find Camera.main.")]
    private Camera mainCamera;


    [Header("Blur Settings")]
    [Tooltip("Scales down the resolution of the texture being blurred for performance.")]
    [SerializeField][Range(0.1f, 1.0f)] private float renderScale = 0.25f; // Lower default for this use case
    [Tooltip("How many times the blur shader is applied.")]
    [SerializeField][Range(1, 5)] private int blurIterations = 1; // Often 1 is enough if shader is good

    private MaskableGraphic targetGraphic; // The Image or RawImage component on this GameObject
    private RenderTexture blurredBackgroundRT;
    private RenderTexture tempRTForBlur;

    // Store original material if using Image component
    private Material originalMaterial;
    private Texture originalTexture; // If RawImage had a texture
    private Sprite originalSprite; // If Image had a sprite

    void Awake()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        targetGraphic = GetComponent<MaskableGraphic>();
        if (targetGraphic == null)
        {
            Debug.LogError("UIBlurBackgroundSelf: No Image or RawImage component found on this GameObject!", this);
            enabled = false;
            return;
        }

        if (blurMaterial == null)
        {
            Debug.LogError("UIBlurBackgroundSelf: BlurMaterial not assigned!", this);
            enabled = false;
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("UIBlurBackgroundSelf: Main Camera not found or assigned!", this);
                enabled = false;
                return;
            }
        }

        // Store original graphic properties
        if (targetGraphic is Image image)
        {
            originalMaterial = image.material;
            originalSprite = image.sprite;
        }
        else if (targetGraphic is RawImage rawImage)
        {
            originalMaterial = rawImage.material;
            originalTexture = rawImage.texture;
        }
    }

    void OnEnable()
    {
        // This method is called when the GameObject (and this script) becomes active.
        // This is where we'll apply the blur.
        ApplyBlur();
    }

    void OnDisable()
    {
        // This is called when the GameObject becomes inactive.
        // We'll revert to the original appearance and release resources.
        RevertAndCleanUp();
    }

    void OnDestroy()
    {
        // Ensure cleanup if the object is destroyed.
        RevertAndCleanUp(true);
    }

    private void ApplyBlur()
    {
        if (!enabled || !gameObject.activeInHierarchy || mainCamera == null || blurMaterial == null || targetGraphic == null)
        {
            return;
        }

        // --- Ensure previous RTs are released ---
        ReleaseRenderTextures();

        int rtWidth = Mathf.Max(1, Mathf.RoundToInt(Screen.width * renderScale));
        int rtHeight = Mathf.Max(1, Mathf.RoundToInt(Screen.height * renderScale));

        blurredBackgroundRT = RenderTexture.GetTemporary(rtWidth, rtHeight, 0, RenderTextureFormat.Default);
        tempRTForBlur = RenderTexture.GetTemporary(rtWidth, rtHeight, 0, RenderTextureFormat.Default);


        // --- 1. Capture Screen Content that would be BEHIND this UI ---
        // This is the trickiest part. For a simple full-screen UI panel,
        // we capture the whole screen *before* this UI is fully drawn opaque.
        // Ideally, this capture happens just before this panel becomes visible.
        // The OnEnable timing might be okay if the panel is activated after the frame is mostly rendered.

        RenderTexture fullScreenCapture = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.Default);
        RenderTexture prevCamTarget = mainCamera.targetTexture;
        RenderTexture prevActiveRT = RenderTexture.active;

        mainCamera.targetTexture = fullScreenCapture;
        mainCamera.Render();
        Graphics.Blit(fullScreenCapture, blurredBackgroundRT); // Copy to our processing RT

        mainCamera.targetTexture = prevCamTarget;
        RenderTexture.active = prevActiveRT;
        RenderTexture.ReleaseTemporary(fullScreenCapture);

        // --- 2. Apply Blur Shader ---
        RenderTexture source = blurredBackgroundRT;
        RenderTexture destination = tempRTForBlur;

        for (int i = 0; i < blurIterations; i++)
        {
            Graphics.Blit(source, destination, blurMaterial);
            RenderTexture tempSwap = source;
            source = destination;
            destination = tempSwap;
        }

        // 'source' now contains the final blurred image.
        // We need to assign this to our targetGraphic.
        // If targetGraphic is an Image, we can't directly assign a RenderTexture to its sprite.
        // So, this script works best if the targetGraphic is a RawImage.
        // If it's an Image, we'd change its material and set the texture on that material.

        if (targetGraphic is RawImage rawImage)
        {
            rawImage.texture = source; // 'source' is the RT we want to keep for display
            rawImage.material = null; // Use default RawImage material or a simple unlit one if needed
                                      // Or, if your blurMaterial is also meant for display (e.g. unlit), use it:
                                      // rawImage.material = blurMaterial;
                                      // Then ensure blurMaterial's _MainTex is set:
                                      // blurMaterial.SetTexture("_MainTex", source);
                                      // For simplicity, let's assume RawImage displays the RT directly.
        }
        else if (targetGraphic is Image image)
        {
            // This is more complex for 'Image' as it uses sprites.
            // A common approach is to create a new material instance for the Image
            // and set the texture on that, or use a custom UI shader that can take a RenderTexture.
            // For simplicity, this script is easier if attached to a RawImage.
            // If you must use Image, you might change its material to one that can sample _MainTex.
            Debug.LogWarning("UIBlurBackgroundSelf works best with a RawImage. Behaviour with Image might be limited or require shader/material adjustments for the Image component itself.");
            // A workaround: set the image's material to your blurMaterial, and the blurMaterial's _MainTex to 'source'
            image.material = blurMaterial; // Ensure blurMaterial has _MainTex property
            blurMaterial.SetTexture("_MainTex", source);
            image.sprite = null; // Clear sprite to use material texture
        }


        // Manage the RTs: 'source' is being used. Release 'destination' if it's different and exists.
        if (source == blurredBackgroundRT && tempRTForBlur != null)
        {
            RenderTexture.ReleaseTemporary(tempRTForBlur);
            tempRTForBlur = null;
        }
        else if (source == tempRTForBlur && blurredBackgroundRT != null)
        {
            // 'source' is tempRTForBlur, means blurredBackgroundRT was the 'destination' in the last swap.
            // We are displaying tempRTForBlur. We should keep it.
            // But our member variable is blurredBackgroundRT. Let's swap them so blurredBackgroundRT holds the final.
            RenderTexture.ReleaseTemporary(blurredBackgroundRT);
            blurredBackgroundRT = source; // Now blurredBackgroundRT holds the one being displayed.
            tempRTForBlur = null;
        }
    }

    private void RevertAndCleanUp(bool isBeingDestroyed = false)
    {
        if (targetGraphic != null)
        {
            // Revert material and texture/sprite
            if (targetGraphic is Image image)
            {
                image.material = originalMaterial;
                image.sprite = originalSprite;
                // If we changed blurMaterial's texture, reset it
                if (blurMaterial != null && blurMaterial.HasProperty("_MainTex"))
                {
                    blurMaterial.SetTexture("_MainTex", null); // Or to its default white texture
                }
            }
            else if (targetGraphic is RawImage rawImage)
            {
                rawImage.texture = originalTexture;
                rawImage.material = originalMaterial;
            }
        }

        ReleaseRenderTextures();

        if (isBeingDestroyed && blurMaterial != null && blurMaterial.HasProperty("_MainTex"))
        {
            // If the material instance itself might be shared or if it's an asset,
            // ensure its _MainTex is cleared if we set it.
            // This is more critical if we were instantiating materials.
            blurMaterial.SetTexture("_MainTex", null);
        }
    }

    private void ReleaseRenderTextures()
    {
        if (blurredBackgroundRT != null)
        {
            RenderTexture.ReleaseTemporary(blurredBackgroundRT);
            blurredBackgroundRT = null;
        }
        if (tempRTForBlur != null)
        {
            RenderTexture.ReleaseTemporary(tempRTForBlur);
            tempRTForBlur = null;
        }
    }
}