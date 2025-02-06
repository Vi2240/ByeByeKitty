using UnityEngine;
using System.Collections;

public class GeneralizedDash2D
{
    public static void Dash2D(float dashSpeed, float dashTime, float dashChargupTime, Vector2 dashDirection, GameObject entityToDash)
    {
        Rigidbody2D rb = entityToDash.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody2D found on the entity to dash.");
            return;
        }

        // Start the dash sequence
        Debug.Log("Starting dash sequence...");
        entityToDash.GetComponent<MonoBehaviour>().StartCoroutine(DashSequence(rb, dashSpeed, dashTime, dashChargupTime, dashDirection));
    }

    private static IEnumerator DashSequence(Rigidbody2D rb, float dashSpeed, float dashTime, float dashChargupTime, Vector2 dashDirection)
    {
        // Chargeup time before the dash
        Debug.Log("Charging up for " + dashChargupTime + " seconds...");
        yield return new WaitForSeconds(dashChargupTime);

        // Calculate the target position
        Debug.Log("Dashing... target pos calc");
        Vector2 startPosition = rb.position;
        Vector2 targetPosition = startPosition + dashDirection.normalized * dashSpeed * dashTime;    

        // Start the dash
        Debug.Log("Dashing... starting dash");
        float elapsedTime = 0f;
        while (elapsedTime < dashTime)
        {
            Debug.Log("Dashing... moving");
            rb.MovePosition(Vector2.Lerp(startPosition, targetPosition, elapsedTime / dashTime));            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final position is set
        rb.MovePosition(targetPosition);
    }
}
