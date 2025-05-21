using UnityEngine;

public class Shield : MonoBehaviour
{
    public int durability = 25;

    // Call this to set the shield's durability
    public void Initialize(int hitPoints)
    {
        durability = hitPoints;
    }

    // When an enemy collides, reduce durability
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Assuming enemies should not pass through the shield if they're not already inside,
            // rely on colliders/physics to block them.
            TakeHit();
        }
    }

    // Reduce durability; destroy the shield if durability is depleted
    public void TakeHit()
    {
        durability--;
        if (durability <= 0)
        {
            Destroy(gameObject);
        }
    }
}