using System.Collections;
using UnityEngine;

public class Skeleton : Enemy
{
    [Header("Skeleton Special")]
    public Sprite headSprite; // Head sprite for near-death state (deprecated if using animator)
    public float dropDistance = 0.5f; // Distance to drop when entering near-death
    public float dropDuration = 0.3f; // Duration of drop animation
    public float floatDistance = 0.5f; // Distance to float up when reviving
    public float floatDuration = 0.5f; // Duration of float animation
    
    [Header("Animation Parameters")]
    [Tooltip("Animator trigger name for entering near-death state.")]
    public string enterNearDeathTriggerName = "EnterNearDeath";
    [Tooltip("Animator trigger name for reviving.")]
    public string reviveTriggerName = "Revive";
    
    private bool hasRevived = false; // Track if already revived once
    private bool isNearDeath = false; // Track if in near-death state
    private Sprite originalSprite; // Store original sprite
    private int deathTurn = -1; // Turn when died (for resurrection timing)
    private bool isInDropAnimation = false; // Track if currently in drop animation
    private bool isInReviveAnimation = false; // Track if currently in revive animation

    protected override void Start()
    {
        maxHP = 10f;
        base.Start();
        
        // Store original sprite
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }
    }

    public override void Die()
    {
        if (isNearDeath)
        {
            // Already in near-death state, truly die now
            base.Die();
            return;
        }

        if (hasRevived)
        {
            // Already revived once, truly die now
            base.Die();
            return;
        }

        // First death - enter near-death state
        EnterNearDeathState();
    }

    private void EnterNearDeathState()
    {
        isNearDeath = true;
        canMove = false;
        
        // Trigger near-death animation
        if (animator != null && !string.IsNullOrEmpty(enterNearDeathTriggerName))
        {
            animator.SetTrigger(enterNearDeathTriggerName);
        }
        
        // Set HP to 30 for near-death state
        currentHP = 30f;
        maxHP = 30f;
        
        // Change sprite to head (only if not using animator)
        if (spriteRenderer != null && headSprite != null && animator == null)
        {
            spriteRenderer.sprite = headSprite;
        }
        
        // Register to TurnManager for resurrection tracking
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.RegisterSkeleton(this);
            deathTurn = TurnManager.Instance.GetCurrentTurn();
        }
        
        // Start drop animation
        StartCoroutine(DropAnimationCoroutine());
    }

    private IEnumerator DropAnimationCoroutine()
    {
        isInDropAnimation = true;
        canMove = false;
        
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.down * dropDistance;
        
        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        transform.position = targetPos;
        isInDropAnimation = false;
    }

    // Called by TurnManager at the start of enemy turn
    public void CheckAndStartRevival()
    {
        if (isNearDeath && !hasRevived)
        {
            int currentTurn = TurnManager.Instance != null ? TurnManager.Instance.GetCurrentTurn() : 0;
            if (currentTurn > deathTurn)
            {
                // Time to revive (at the same time as other enemies move down)
                StartCoroutine(ReviveCoroutine());
            }
        }
    }

    private IEnumerator ReviveCoroutine()
    {
        hasRevived = true;
        isNearDeath = false;
        isInReviveAnimation = true;
        canMove = false;
        
        // Trigger revive animation
        if (animator != null && !string.IsNullOrEmpty(reviveTriggerName))
        {
            animator.SetTrigger(reviveTriggerName);
        }
        
        // Float up animation
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * floatDistance;
        
        float elapsed = 0f;
        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / floatDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        transform.position = targetPos;
        
        // Restore original sprite (only if not using animator)
        if (spriteRenderer != null && originalSprite != null && animator == null)
        {
            spriteRenderer.sprite = originalSprite;
        }
        
        // Restore full HP
        maxHP = 10f;
        currentHP = maxHP;
        
        isInReviveAnimation = false;
        // Re-enable movement
        canMove = true;
        
        // Unregister from TurnManager
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.UnregisterSkeleton(this);
        }
    }

    public override IEnumerator MoveDown()
    {
        // Don't move if in near-death state
        if (isNearDeath)
        {
            yield break;
        }
        
        // Don't move if in drop or revive animation
        if (isInDropAnimation || isInReviveAnimation)
        {
            yield break;
        }
        
        // Check if movement is disabled
        if (!canMove)
        {
            yield break;
        }
        
        // Normal movement for alive state
        yield return StartCoroutine(base.MoveDown());
    }

    public bool IsNearDeath()
    {
        return isNearDeath;
    }
}