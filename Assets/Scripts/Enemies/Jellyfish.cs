using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jellyfish : Enemy
{
    [Header("Jellyfish Special")]
    public float speedReductionFactor = 0.9f; // Speed reduction when ball passes through (0.9 = 10% slower)
    public float transparencyAlpha = 0.5f; // Alpha value when ball is behind jellyfish (0.5 = 50% transparent)
    
    private HashSet<PinBall> passingBalls = new HashSet<PinBall>(); // Track balls currently passing through
    private SpriteRenderer jellyfishRenderer;
    private int originalSortingOrder;

    protected override void Start()
    {
        // Call base Enemy Start to initialize common properties
        base.Start();
        
        // Get jellyfish sprite renderer for sorting order reference
        jellyfishRenderer = GetComponent<SpriteRenderer>();
        if (jellyfishRenderer != null)
        {
            originalSortingOrder = jellyfishRenderer.sortingOrder;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if colliding with a pinball
        PinBall ball = other.GetComponent<PinBall>();
        if (ball != null)
        {
            // Deal damage
            TakeDamage(ball.damage);
            
            // Add ball to passing list and start transparency effect
            if (!passingBalls.Contains(ball))
            {
                passingBalls.Add(ball);
                StartCoroutine(HandleBallPassing(ball));
            }
        }
    }

    private IEnumerator HandleBallPassing(PinBall ball)
    {
        if (ball == null) yield break;
        
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        SpriteRenderer ballRenderer = ball.GetComponent<SpriteRenderer>();
        
        if (ballRb == null || ballRenderer == null) yield break;
        
        // Reduce ball speed
        ballRb.velocity *= speedReductionFactor;
        
        // Store original ball properties
        Color originalBallColor = ballRenderer.color;
        int originalBallSortingOrder = ballRenderer.sortingOrder;
        
        // Continuously check if ball is behind jellyfish
        while (ball != null && IsBallBehindJellyfish(ball))
        {
            // Ball is behind: make it semi-transparent and lower sorting order
            if (ballRenderer != null)
            {
                Color semiTransparent = originalBallColor;
                semiTransparent.a = transparencyAlpha;
                ballRenderer.color = semiTransparent;
                
                // Set ball sorting order to be below jellyfish
                if (jellyfishRenderer != null)
                {
                    ballRenderer.sortingOrder = jellyfishRenderer.sortingOrder - 1;
                }
            }
            
            yield return null;
        }
        
        // Ball is no longer behind: restore original properties
        if (ball != null && ballRenderer != null)
        {
            ballRenderer.color = originalBallColor;
            ballRenderer.sortingOrder = originalBallSortingOrder;
        }
        
        // Remove from passing list
        passingBalls.Remove(ball);
    }

    private bool IsBallBehindJellyfish(PinBall ball)
    {
        if (ball == null) return false;
        
        // Check if ball's Y position is below jellyfish's Y position
        return ball.transform.position.y < transform.position.y;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        foreach (var ball in passingBalls)
        {
            if (ball != null)
            {
                SpriteRenderer ballRenderer = ball.GetComponent<SpriteRenderer>();
                if (ballRenderer != null)
                {
                    Color originalColor = ballRenderer.color;
                    originalColor.a = 1f;
                    ballRenderer.color = originalColor;
                }
            }
        }
        passingBalls.Clear();
    }
}

