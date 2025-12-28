using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBlock : MonoBehaviour
{
    [Header("Ice Block Settings")]
    public float maxHP = 8f;
    public float currentHP;
    
    [Header("Stuck Balls")]
    private List<PinBall> stuckBalls = new List<PinBall>();
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float hitFlashDuration = 0.1f;

    void Start()
    {
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        
        // Play hit flash effect
        StartCoroutine(HitFlashCoroutine());
        
        if (currentHP <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitFlashCoroutine()
    {
        if (spriteRenderer == null) yield break;
        
        spriteRenderer.color = Color.Lerp(originalColor, Color.cyan, 0.5f);
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }

    public void StickBall(PinBall ball)
    {
        if (ball != null && !stuckBalls.Contains(ball))
        {
            stuckBalls.Add(ball);
            
            // Stop the ball's movement
            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            if (ballRb != null)
            {
                ballRb.velocity = Vector2.zero;
            }
            
            // Make ball a child of ice block (visual attachment)
            ball.transform.SetParent(transform);
            
            // Remove ball from active list so turn can end
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.UnregisterBall(ball);
            }

            // Reduce player's available ball count
            PlayerCharacter player = FindObjectOfType<PlayerCharacter>();
            if (player != null)
            {
                player.AddStuckBallPenalty(1);
            }
        }
    }

    private void Die()
    {
        // Return all stuck balls to player
        ReturnStuckBalls();
        
        // Destroy ice block
        Destroy(gameObject);
    }

    private void ReturnStuckBalls()
    {
        PlayerCharacter player = FindObjectOfType<PlayerCharacter>();
        if (player != null)
        {
            player.RemoveStuckBallPenalty(stuckBalls.Count);
        }
        
        // Destroy all stuck balls
        foreach (PinBall ball in stuckBalls)
        {
            if (ball != null)
            {
                Destroy(ball.gameObject);
            }
        }
        
        stuckBalls.Clear();
    }

    void OnDestroy()
    {
        // Return balls if destroyed unexpectedly
        if (stuckBalls.Count > 0)
        {
            ReturnStuckBalls();
        }
    }

    // Check if this ice block is blocking an enemy
    public bool IsBlockingEnemy(Enemy enemy)
    {
        Collider2D iceCollider = GetComponent<Collider2D>();
        Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
        
        if (iceCollider == null || enemyCollider == null) return false;
        
        return iceCollider.bounds.Intersects(enemyCollider.bounds);
    }
}

