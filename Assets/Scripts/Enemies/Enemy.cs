using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [Header("Base Stats")]
    public float maxHP = 10f;
    
    [Header("Movement")]
    public float moveDistance = 1f;
    public bool canMove = true;
    public float moveDuration = 0.5f;
    public float knockbackExtraDistance = 0.2f;

    [Header("Animation")]
    public string isMovingParameterName = "IsMoving";
    
    [Header("Hit Effect")]
    public float hitFlashDuration = 0.1f;
    
    [Header("Death Effect")]
    public float deathRotationSpeed = 720f; // Degrees per second
    public float deathUpwardSpeed = 5f; // Units per second
    public float deathDuration = 2f; // How long before destroying
    
    [Header("Boundary Check")]
    public Transform bottomBoundary; // Reference to bottom boundary
    public float chargeSpeed = 15f; // Speed when charging at player
    
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;

    [HideInInspector] public float currentHP;
    protected bool isMoving = false;
    private bool hasKnockedBackThisTurn = false;
    private bool isChargingAtPlayer = false;

    protected virtual void Start()
    {
        currentHP = maxHP;
        
        // Register enemy for level progression tracking
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterEnemy(this);
        }
        
        // Auto-find Animator component
        animator = GetComponent<Animator>();
        
        // Auto-find SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Initialize animation state to idle
        if (animator != null)
        {
            animator.SetBool(isMovingParameterName, false);
        }
    }

    public void TakeDamage(float dmg)
    {
        float oldHP = currentHP;
        currentHP -= dmg;
        
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
        
        // Flash light red (mix original color with red)
        spriteRenderer.color = Color.Lerp(originalColor, Color.red, 0.5f);
        yield return new WaitForSeconds(hitFlashDuration);
        
        // Return to original color
        spriteRenderer.color = originalColor;
    }

    public virtual IEnumerator MoveDown()
    {
        if (canMove && !isMoving)
        {
            isMoving = true;
            hasKnockedBackThisTurn = false;
            
            // Start movement animation
            if (animator != null)
            {
                animator.SetBool(isMovingParameterName, true);
            }
            
            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + Vector3.down * moveDistance;

            float elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / moveDuration;
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                
                // Real-time collision check during movement
                CheckPlayerCollision();
                
                // Check for ice block collision
                if (CheckIceBlockCollision())
                {
                    // Movement stopped by ice block
                    isMoving = false;
                    if (animator != null)
                    {
                        animator.SetBool(isMovingParameterName, false);
                    }
                    yield break;
                }
                
                // Check bottom boundary during movement
                if (CheckBottomBoundary())
                {
                    // Enemy reached boundary, stop movement
                    isMoving = false;
                    if (animator != null)
                    {
                        animator.SetBool(isMovingParameterName, false);
                    }
                    yield break;
                }
                
                yield return null;
            }

            transform.position = targetPos;
            isMoving = false;
            
            // Stop movement animation
            if (animator != null)
            {
                animator.SetBool(isMovingParameterName, false);
            }
            
            // Final check if enemy reached bottom boundary
            CheckBottomBoundary();
        }
    }
    
    protected virtual bool CheckIceBlockCollision()
    {
        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider == null) return false;
        
        // Find all ice blocks in scene
        IceBlock[] iceBlocks = FindObjectsOfType<IceBlock>();
        
        foreach (IceBlock iceBlock in iceBlocks)
        {
            if (iceBlock != null && iceBlock.IsBlockingEnemy(this))
            {
                // Check enemy type
                Skeleton skeleton = this as Skeleton;
                Golem golem = this as Golem;
                FrostDragon frostDragon = this as FrostDragon;
                
                if (skeleton != null)
                {
                    // Skeleton: cannot advance, stop movement
                    return true;
                }
                else if (golem != null || frostDragon != null)
                {
                    // Golem or FrostDragon: destroy ice block
                    iceBlock.TakeDamage(iceBlock.maxHP); // Deal enough damage to destroy it
                    return false; // Continue movement
                }
            }
        }
        
        return false;
    }

    private bool CheckBottomBoundary()
    {
        if (bottomBoundary == null) return false;
        
        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider == null) return false;
        
        Collider2D boundaryCollider = bottomBoundary.GetComponent<Collider2D>();
        if (boundaryCollider == null) return false;
        
        // Method 1: Use bounds intersection (most reliable, doesn't depend on physics)
        Bounds enemyBounds = enemyCollider.bounds;
        Bounds boundaryBounds = boundaryCollider.bounds;
        bool isOverlapping = enemyBounds.Intersects(boundaryBounds);
        
        // Method 2: Use Physics2D.OverlapBox as additional check
        // Get all colliders overlapping with enemy's bounds
        Collider2D[] overlaps = Physics2D.OverlapBoxAll(
            enemyCollider.bounds.center,
            enemyCollider.bounds.size,
            0f
        );
        
        // Check if boundary collider is in the overlaps
        foreach (Collider2D overlap in overlaps)
        {
            if (overlap == boundaryCollider)
            {
                isOverlapping = true;
                break;
            }
        }
        
        if (isOverlapping && !isChargingAtPlayer)
        {        
            // Start charging at player
            StartCoroutine(ChargeAtPlayerCoroutine());
            
            return true; // Return true to indicate boundary was reached
        }
        
        return false;
    }
    
    private IEnumerator ChargeAtPlayerCoroutine()
    {
        isChargingAtPlayer = true;
        
        // Disable collision with other objects (but keep for player detection)
        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider != null)
        {
            // Set to trigger so it doesn't collide with walls/other enemies
            enemyCollider.isTrigger = true;
        }
        
        // Disable movement and animation
        canMove = false;
        isMoving = false;
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        // Find player
        PlayerCharacter player = FindObjectOfType<PlayerCharacter>();
        if (player == null)
        {
            // No player found, just destroy
            Destroy(gameObject);
            yield break;
        }
        
        // Charge at player
        Vector3 startPos = transform.position;
        float damage = currentHP; // Store remaining HP as damage
        
        while (player != null && !player.IsDead)
        {
            // Calculate direction to player
            Vector3 direction = (player.transform.position - transform.position).normalized;
            
            // Move towards player
            transform.position += direction * chargeSpeed * Time.deltaTime;
            
            // Check if reached player
            if (enemyCollider != null)
            {
                Collider2D playerCollider = player.GetComponent<Collider2D>();
                if (playerCollider != null && enemyCollider.IsTouching(playerCollider))
                {
                    // Hit player, deal damage
                    player.TakeDamageWithoutKnockback(damage);
                    
                    // Destroy enemy
                    Destroy(gameObject);
                    yield break;
                }
            }
            
            yield return null;
        }
        
        // Player died or disappeared, destroy enemy
        Destroy(gameObject);
    }

    protected virtual void CheckPlayerCollision()
    {
        // Prevent multiple knockbacks in the same turn
        if (hasKnockedBackThisTurn) return;

        // Use Collider2D to check collision with player
        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider == null) return;

        Collider2D playerCollider = Physics2D.OverlapBox(
            enemyCollider.bounds.center,
            enemyCollider.bounds.size,
            0f,
            LayerMask.GetMask("Player")
        );

        if (playerCollider != null)
        {
            PlayerCharacter player = playerCollider.GetComponent<PlayerCharacter>();
            if (player != null && TurnManager.Instance != null)
            {
                // Calculate knockback distance: moveDistance + extra
                float knockbackDistance = GetKnockbackDistance();
                TurnManager.Instance.HandleEnemyPlayerCollision(this, player, knockbackDistance);
                hasKnockedBackThisTurn = true;
            }
        }
    }

    protected virtual float GetKnockbackDistance()
    {
        return moveDistance + knockbackExtraDistance;
    }

    public virtual void Die()
    {
        // Start death effect coroutine instead of immediately destroying
        StartCoroutine(DeathEffectCoroutine());
    }

    protected virtual void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterEnemy(this);
        }
    }
    
    private IEnumerator DeathEffectCoroutine()
    {
        // Disable collision
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // Disable movement and other behaviors
        canMove = false;
        isMoving = false;
        
        // Disable animator if exists
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        // Calculate upward direction (random angle between -45 and 45 degrees from vertical)
        float randomAngle = Random.Range(-45f, 45f);
        Vector2 deathDirection = Quaternion.Euler(0, 0, randomAngle) * Vector2.up;
        
        // Store initial position
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        
        // Death animation: rotate and move upward
        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;
            
            // Rotate
            transform.Rotate(0, 0, deathRotationSpeed * Time.deltaTime);
            
            // Move upward
            transform.position = startPos + (Vector3)(deathDirection * deathUpwardSpeed * elapsed);
            
            yield return null;
        }
        
        // Destroy after effect
        Destroy(gameObject);
    }
}
