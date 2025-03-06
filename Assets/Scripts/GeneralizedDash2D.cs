using UnityEngine;
using System.Collections;

public static class GeneralizedDash2D
{
    /// <summary>
    /// Initiates a dash after a charge-up delay.
    /// This coroutine moves the entity with a kinematic Rigidbody2D in the given direction.
    /// If not obstructed, the entity moves dashSpeed * dashTime units in that direction.
    /// </summary>
    /// <param name="dashDistance">Distance of how far to dash.</param>
    /// <param name="dashTime">Duration of the dash in seconds.</param>
    /// <param name="dashChargeUpTime">Delay before the dash starts in seconds.</param>
    /// <param name="dashDirection">The direction the entity should dash toward.</param>
    /// <param name="entity">The GameObject that is dashing.</param>
    /// <param name="dashingFlag">A wrapped boolean flag that is set to true while dashing.</param>
    public static IEnumerator Dash2D(float dashDistance, float dashTime, float dashChargeUpTime, Vector2 dashDirection, GameObject entity, Wrapper<bool> dashingFlag)
    {
        // Try to get the Rigidbody2D component.
        Rigidbody2D rb = entity.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody2D found on the entity GameObject.");
            yield break;    
        }

        // Sets the dashing flag to true signafying that dashing is in progress.
        dashingFlag.value = true;

        // Wait for the dash charge-up time.
        yield return new WaitForSeconds(dashChargeUpTime);

        // Calculate the required speed to travel the full distance in the given time.
        float dashSpeed = dashDistance / dashTime;

        // Using FixedUpdate intervals for physics-based movement.
        float elapsedTime = 0f;
        while (elapsedTime < dashTime)
        {
            // Calculate how far to move this physics step.
            float stepDistance = dashSpeed * Time.fixedDeltaTime;

            // Move the Rigidbody2D by the computed step along the dashDirection.
            rb.MovePosition(rb.position + dashDirection * stepDistance);

            // Increment the elapsed time.
            elapsedTime += Time.fixedDeltaTime;

            // Wait for the next physics update.
            yield return new WaitForFixedUpdate();
        }

        // Sets the dashing flag to false signafying that dashing is complete.
        dashingFlag.value = false;
    }
}
