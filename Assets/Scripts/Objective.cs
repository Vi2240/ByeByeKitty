using System.Collections;
using UnityEngine;

public class Objective : MonoBehaviour
{
    // ... (Keep all your existing Header variables and other fields) ...
    [Header("Tree Variables")]
    [SerializeField] Enemy_HealthBar healthBar;
    [SerializeField] GameObject numberEffect;
    [SerializeField] GameObject healingEffect;
    [SerializeField] GameObject waterEffect;
    [SerializeField] float waterEffectTime = 2.0f; // Linger time for visual, and interval for sound
    [SerializeField] float maxHp;
    [SerializeField] float healSpeed;
    [SerializeField] float treeHeal;
    [SerializeField] bool enemyKillzone;
    float currentHp;
    bool treeAlive = true;    

    [Header("Fire Variables")]
    [SerializeField] bool canFireBeExtinguished = true;
    [SerializeField] float maxFireHp;
    [SerializeField] float fireHeal;
    [SerializeField] float fireHealSpeed;
    [SerializeField] float burningDmg;
    [SerializeField] float burnSpeed;
    [SerializeField] float BurnEffectScaleMult = 1;
    [SerializeField] float scaleDownDuration = 4f;
    [SerializeField, Tooltip("1.0 is completely black.")] float maxShaderBurnValue = 0.99f;
    [SerializeField] SpriteRenderer spriteRenderer;
    Material materialInstance;
    private static readonly int BurnAmountPropertyID = Shader.PropertyToID("_BurnAmount");
    float fireIntensityPercentageFactor;
    float fireHP;
    Vector3 maxBurnEffectScale_Vec;

    [Tooltip("How long (in seconds) after a fire is extinguished before it can start growing its HP again.")]
    [SerializeField] float delayFireGrowthAfterExtinguishTime = 1.5f;
    [SerializeField] bool delayFireGrowthAfterExtinguish = true;
    float fireGrowthDelayedUntil;

    [Header("Fire Activation Variables")]
    [SerializeField] GameObject burnEffect;
    [SerializeField] float rekindleFireDelay = 5f;
    [SerializeField] UnityEngine.Transform fireCircle;
    [SerializeField] float maxScale = 1f;
    [SerializeField] float requiredHoldTime = 5.0f;
    float holdTimer = 0f;

    [SerializeField] GameObject winCanvas;
    [SerializeField] GameObject waveManagerObject;

    bool playersInZone;
    Wrapper<bool> isBurning;
    bool gameWon;

    WaveManager waveManager;
    AudioSource burningAudioSource;

    // --- Variables for improved water effect & sound ---
    bool isWaterEffectVisuallyActive = false;
    Coroutine waterVisualLingerCoroutine = null;
    Coroutine waterSoundLoopCoroutine = null; // New coroutine for sound
    // --- End new variables ---

    void Start()
    {
        winCanvas.SetActive(false);

        // HP and health bar
        fireHP = 0;
        currentHp = maxHp;
        healthBar.SetHealth(currentHp, maxHp);

        // Burning
        materialInstance = spriteRenderer.material;
        UpdateBurnVisuals();
        isBurning = new Wrapper<bool>(false);
        fireIntensityPercentageFactor = 0f;
        delayFireGrowthAfterExtinguish = true;
        fireGrowthDelayedUntil = 0f;

        // Other
        gameWon = false;
        treeAlive = true;

        if (waveManagerObject != null)
        {
            waveManager = waveManagerObject.GetComponent<WaveManager>();
            if (waveManager == null)
                Debug.LogError("WaveManager component not found on waveManagerObject!", this);
        }
        else Debug.LogError("waveManagerObject is not assigned in the Inspector!", this);


        if (burnEffect != null)
        {
            burnEffect.SetActive(false);
            maxBurnEffectScale_Vec = burnEffect.transform.localScale * BurnEffectScaleMult;
        }
        else Debug.LogError("BurnEffect GameObject is not assigned in the Inspector!", this);

        if (waterEffect != null)
        {
            waterEffect.SetActive(false); // Ensure it's off at start
        }
        else Debug.LogError("WaterEffect GameObject is not assigned in the Inspector!", this);

        if (healingEffect != null)
        {
            healingEffect.SetActive(false);
        }
        else Debug.LogError("HealingEffect GameObject is not assigned in the Inspector!", this);


        healSpeed       = (healSpeed == 0)      ? 0 : 1 / healSpeed;
        burnSpeed       = (burnSpeed == 0)      ? 0 : 1 / burnSpeed;
        fireHealSpeed   = (fireHealSpeed == 0)  ? 0 : 1 / fireHealSpeed;
    }

    float rekindleFireTimer = 0f;
    void Update()
    {
        if (!treeAlive) return;

        if (!playersInZone || isBurning.value || !Input.GetKey(KeyCode.E) || rekindleFireTimer > Time.time)
        {
            holdTimer = 0f;
            if (fireCircle != null) fireCircle.localScale = Vector3.zero;
            return;
        }

        holdTimer += Time.deltaTime;
        float scale = Mathf.Clamp01(holdTimer / requiredHoldTime) * maxScale;
        if (fireCircle != null) fireCircle.localScale = new Vector3(scale, scale, scale);

        if (holdTimer >= requiredHoldTime)
        {
            if (AudioPlayer.Current != null)
            {
                burningAudioSource = AudioPlayer.Current.PlayLoopingSfx("Fire_Sound", transform.position);
            }
            isBurning.value = true;
            fireHP = maxFireHp / 4;
            if (waveManager != null) waveManager.StartContinuousWaves(gameObject.transform, isBurning);
            rekindleFireTimer = Time.time + rekindleFireDelay;
            holdTimer = 0f;
            if (fireCircle != null) fireCircle.localScale = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        if (!treeAlive) return;

        if (isBurning.value)
        {
            if (healingEffect != null && healingEffect.activeSelf) healingEffect.SetActive(false);
            fireIntensityPercentageFactor = fireHP / maxFireHp;
            HandleBurningDamage(Time.fixedDeltaTime);
            HandleFireGrowth(Time.fixedDeltaTime);
            return;
        }

        if (burnEffect != null && burnEffect.activeSelf) burnEffect.SetActive(false);
        if (currentHp < maxHp)
        {
            HandleHealing(Time.fixedDeltaTime);
        }
        else
        {
            if (healingEffect != null && healingEffect.activeSelf) healingEffect.SetActive(false);
        }
    }

    float burnDMGTimer = 0f;
    void HandleBurningDamage(float dt)
    {
        burnDMGTimer += dt;
        float currentBurnTickRate = (fireIntensityPercentageFactor > 0.01f) ? burnSpeed / fireIntensityPercentageFactor : burnSpeed * 100f;
        if (currentBurnTickRate <= 0) currentBurnTickRate = float.MaxValue;

        while (burnDMGTimer >= currentBurnTickRate)
        {
            burnDMGTimer -= currentBurnTickRate;
            currentHp -= burningDmg;

            healthBar.SetHealth(currentHp, maxHp);
            UpdateBurnVisuals();

            if (numberEffect != null)
            {
                var dmgnr = Instantiate(numberEffect, transform.position, Quaternion.identity);
                dmgnr.GetComponent<FloatingHealthNumber>()?.SetText(burningDmg.ToString(), 1);
            }
            if (currentHp <= 0 && treeAlive)
            {
                treeAlive = false;
                currentHp = 0;
                StartCoroutine(ScaleDownBurnEffect(scaleDownDuration));
                if (waveManager != null) waveManager.StartBossWave();
            }
        }
    }

    private void UpdateBurnVisuals()
    {
        if (materialInstance == null)
        {
            // This check is important if Awake had an issue
            Debug.LogWarning("Material instance not found, cannot update burn visuals for " + gameObject.name);
            return;
        }
        if (maxHp <= 0) // Prevent division by zero or nonsensical values
        {
            materialInstance.SetFloat(BurnAmountPropertyID, 0f);
            return;
        }

        // Calculate the proportion of health lost (0 = full health, 1 = no health)
        float healthLostProportion = 1.0f - (Mathf.Clamp(currentHp, 0f, maxHp) / maxHp);

        // Scale this proportion to our desired shader burn range [0, maxShaderBurnValue]
        float targetBurnAmount = healthLostProportion * maxShaderBurnValue;

        materialInstance.SetFloat(BurnAmountPropertyID, targetBurnAmount);
    }

    float growthTimer = 0f;
    void HandleFireGrowth(float dt)
    {
        if (delayFireGrowthAfterExtinguish && Time.time < fireGrowthDelayedUntil) return;        

        growthTimer += dt;
        if (fireHealSpeed <= 0) return;

        while (growthTimer >= fireHealSpeed)
        {
            growthTimer -= fireHealSpeed;
            fireHP += fireHeal;
            fireHP = Mathf.Clamp(fireHP, 0, maxFireHp);
        }
        UpdateBurnEffectScale();
    }

    float healTimer = 0f;
    void HandleHealing(float dt)
    {
        if (healSpeed <= 0) return;

        healTimer += dt;
        if (healingEffect != null && !healingEffect.activeSelf) healingEffect.SetActive(true);
        while (healTimer >= healSpeed)
        {
            healTimer -= healSpeed;
            currentHp += treeHeal;
            currentHp = Mathf.Clamp(currentHp, 0, maxHp);

            healthBar.SetHealth(currentHp, maxHp);
            UpdateBurnVisuals();

            if (numberEffect == null) return;            
            var healnr = Instantiate(numberEffect, transform.position, Quaternion.identity);
            healnr.GetComponent<FloatingHealthNumber>()?.SetText(treeHeal.ToString(), 2);            
        }
    }

    void UpdateBurnEffectScale()
    {
        if (burnEffect == null) return;

        if (fireIntensityPercentageFactor > 0 && !burnEffect.activeSelf)
        {
            burnEffect.SetActive(true);
        }
        else if (fireIntensityPercentageFactor <= 0 && burnEffect.activeSelf)
        {
            burnEffect.SetActive(false);
        }

        if (burnEffect.activeSelf)
        {
            burnEffect.transform.localScale = maxBurnEffectScale_Vec * fireIntensityPercentageFactor;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playersInZone = true;
        }
        if (other.CompareTag("Enemy"))
        {
            if (enemyKillzone)
            {
                Destroy(other.gameObject);
            }
            else
            {
                other.gameObject.GetComponent<EnemyStopFire>()?.SetInObjectiveZone(true, this.gameObject);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playersInZone = false;
        }
        if (other.CompareTag("Enemy") && !enemyKillzone)
        {
            other.gameObject.GetComponent<EnemyStopFire>()?.SetInObjectiveZone(false, this.gameObject);
        }
    }

    public Wrapper<bool> GetIsBurning()
    {
        return isBurning;
    }

    public bool GetIsBurningState()
    {
        return isBurning.value;
    }

    public bool IsTreeAlive() { return treeAlive; }

    public void FireExtinguish(float fireStoppingPower)
    {
        if (!canFireBeExtinguished || !isBurning.value || !treeAlive) return;

        fireHP -= fireStoppingPower;
        fireIntensityPercentageFactor = fireHP / maxFireHp;
        UpdateBurnEffectScale();
        fireGrowthDelayedUntil = Time.time + delayFireGrowthAfterExtinguishTime;

        // --- Visual Water Effect Logic ---
        if (waterEffect != null)
        {
            if (!isWaterEffectVisuallyActive) // Only activate visuals if they are off
            {
                waterEffect.SetActive(true);
                isWaterEffectVisuallyActive = true;

                // Start the sound loop when visual effect first activates
                if (waterSoundLoopCoroutine == null)
                {
                    waterSoundLoopCoroutine = StartCoroutine(WaterSoundLoopRoutine());
                }
            }

            // Always refresh the visual linger timer
            if (waterVisualLingerCoroutine != null)
            {
                StopCoroutine(waterVisualLingerCoroutine);
            }
            waterVisualLingerCoroutine = StartCoroutine(WaterVisualLingerRoutine());
        }
        // --- End Visual Water Effect Logic ---

        if (fireHP <= 0)
        {
            fireHP = 0f;
            healTimer = 0f;
            growthTimer = 0f;
            burnDMGTimer = 0f;
            isBurning.value = false;

            if (burningAudioSource != null && AudioPlayer.Current != null)
            {
                AudioPlayer.Current.StopLoopingSfx(burningAudioSource);
                burningAudioSource = null;
            }
            // The visual water effect will turn off via its linger coroutine.
            // The sound loop will stop because isWaterEffectVisuallyActive will become false.
        }
    }

    // Add this new coroutine
    IEnumerator ScaleDownBurnEffect(float duration)
    {
        if (burnEffect == null || !burnEffect.activeSelf)
        {
            if (burnEffect != null) burnEffect.SetActive(false); // Ensure it's off if not already
            yield break; // Exit if no effect or it's already inactive
        }

        Vector3 initialScale = burnEffect.transform.localScale;
        Vector3 targetScale = Vector3.zero; // Or a very small scale like new Vector3(0.01f, 0.01f, 0.01f)
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            burnEffect.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        burnEffect.transform.localScale = targetScale; // Ensure it reaches the target scale
        burnEffect.SetActive(false); // Finally, deactivate it
    }

    IEnumerator WaterVisualLingerRoutine()
    {
        yield return new WaitForSeconds(waterEffectTime);
        // This routine will only complete if not restarted by another FireExtinguish call
        if (waterEffect != null) waterEffect.SetActive(false);
        isWaterEffectVisuallyActive = false; // Signal that visuals are off
        waterVisualLingerCoroutine = null;

        // If the visual effect is turning off, ensure the sound loop also stops
        if (waterSoundLoopCoroutine != null)
        {
            StopCoroutine(waterSoundLoopCoroutine);
            waterSoundLoopCoroutine = null;
        }
    }

    IEnumerator WaterSoundLoopRoutine()
    {
        while (isWaterEffectVisuallyActive) // Loop as long as the visual effect is supposed to be active
        {
            if (AudioPlayer.Current != null)
            {
                AudioPlayer.Current.PlaySfxAtPoint("ExtinguishSound", transform.position, 0.5f);
            }
            yield return new WaitForSeconds(waterEffectTime); // Wait for the interval
        }
        waterSoundLoopCoroutine = null; // Clear reference when loop ends
    }

    // ... WinGame, RekindleFireTimmer ...
    // (These methods remain unchanged)
    IEnumerator WinGame()
    {
        if (gameWon) yield break;
        gameWon = true;
        if (winCanvas != null) winCanvas.SetActive(true);
        yield return new WaitForSeconds(5);
        // Loader.LoadNetwork(Loader.Scene.MenuScene);
    }
}