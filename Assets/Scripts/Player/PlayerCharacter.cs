using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCharacter : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    [Header("Shooting")]
    public GameObject pinBallPrefab;
    public float ballSpeed = 10f;
    public int ballsPerTurn = 2;

    [Header("Health")]
    public float maxHP = 10f;
    public float currentHP;

    [Header("Knockback")]
    public float knockbackSpeed = 10f;
    public float hitFlashDuration = 0.2f;
    public int hitFlashCount = 2;
    
    [Header("Death Effect")]
    public float deathRotationSpeed = 720f; // Degrees per second
    public float deathUpwardSpeed = 5f; // Units per second
    public float deathDuration = 2f; // How long before destroying

    [Header("Animation")]
    public Animator animator;
    public float attackAnimationDuration = 0.3f;
    public string attackTriggerName = "AttackUp";  // Default

    [Header("Trajectory Prediction")]
    public TrajectoryPredictor trajectoryPredictor;

    private int ballsShotThisTurn = 0;
    private int currentTurnMaxShots = 2; // Shots allowed for the current turn
    private int stuckBallPenalty = 0; // Balls temporarily locked by ice blocks
    private bool canMove = true;
    private bool canShoot = true;
    private bool isBeingKnockedBack = false;
    private bool isDead = false;
    private bool isPaused = false;
    public bool IsDead { get { return isDead; } }
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private int currentDirection = 1; // 0=Up, 1=Down, 2=Left, 3=Right
    private float lastMoveX = 0f;
    private float lastMoveY = -1f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
    }

        // Auto-find Animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Auto-find TrajectoryPredictor if not assigned
        if (trajectoryPredictor == null)
        {
            trajectoryPredictor = GetComponent<TrajectoryPredictor>();
        }

        // Sync ballSpeed with TrajectoryPredictor
        if (trajectoryPredictor != null)
        {
            trajectoryPredictor.SetBallSpeed(ballSpeed);
        }

        if (animator != null)
        {
            animator.SetFloat("MoveX", lastMoveX);
            animator.SetFloat("MoveY", lastMoveY);
            animator.SetBool("IsMoving", false);
        }

        currentHP = maxHP;
        currentTurnMaxShots = ballsPerTurn;
    }

    void Update()
    {
        // Don't update if dead
        if (isDead)
        {
            return;
        }

        // Don't update if paused
        if (isPaused)
        {
            rb.velocity = Vector2.zero;
            SetIdleAnim();
            return;
        }
        
        // Don't interfere with knockback
        if (isBeingKnockedBack)
        {
            rb.velocity = Vector2.zero;
            SetIdleAnim();
            return;
        }

        if (TurnManager.Instance == null || !TurnManager.Instance.IsPlayerTurn())
        {
            rb.velocity = Vector2.zero;
            SetIdleAnim();
            return;
        }

        // Don't allow movement or shooting while selecting buff
        if (BuffManager.Instance != null && BuffManager.Instance.IsSelectingBuff())
        {
            rb.velocity = Vector2.zero;
            SetIdleAnim();
            // Still allow shooting check to run (it will be blocked inside)
        }

        // ========= Movement =========
        float horizontal = 0f;
        float vertical = 0f;
        bool isMoving = false;

        if (canMove && (BuffManager.Instance == null || !BuffManager.Instance.IsSelectingBuff()))
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
            Vector2 movement = new Vector2(horizontal, vertical).normalized;
            rb.velocity = movement * moveSpeed;
            isMoving = movement.magnitude > 0.1f;
        }
        else
        {
            rb.velocity = Vector2.zero;
            isMoving = false;
        }

        // ========= Animation (MoveX/MoveY/IsMoving) =========
        if (animator != null)
        {
            float moveX;
            float moveY;

            if (isMoving)
            {
                if (Mathf.Abs(vertical) >= Mathf.Abs(horizontal) && Mathf.Abs(vertical) > 0.1f)
                {
                    lastMoveX = 0f;
                    lastMoveY = vertical > 0 ? 1f : -1f;
                }
                else if (Mathf.Abs(horizontal) > 0.1f)
                {
                    lastMoveY = 0f;
                    lastMoveX = horizontal > 0 ? 1f : -1f;
                }

                moveX = lastMoveX;
                moveY = lastMoveY;
            }
            else
            {
                moveX = lastMoveX;
                moveY = lastMoveY;

                if (Mathf.Abs(moveX) < 0.1f && Mathf.Abs(moveY) < 0.1f)
                {
                    moveX = 0f;
                    moveY = -1f;
                    lastMoveX = 0f;
                    lastMoveY = -1f;
                }
            }

            animator.SetFloat("MoveX", moveX);
            animator.SetFloat("MoveY", moveY);
            animator.SetBool("IsMoving", isMoving);
        }

        // ========= Trajectory Prediction =========
        if (canShoot && ballsShotThisTurn < currentTurnMaxShots && !isPaused)
        {
            // Don't show trajectory if clicking on UI or selecting buff
            if (!IsPointerOverUI() && (BuffManager.Instance == null || !BuffManager.Instance.IsSelectingBuff()))
            {
                UpdateTrajectory();
            }
            else
            {
                HideTrajectory();
            }
        }
        else
        {
            HideTrajectory();
        }

        // ========= Shooting =========
        if (canShoot && Input.GetMouseButtonDown(0) && ballsShotThisTurn < currentTurnMaxShots)
        {
            // Don't shoot if clicking on UI or selecting buff
            if (IsPointerOverUI() || (BuffManager.Instance != null && BuffManager.Instance.IsSelectingBuff()))
            {
                return;
            }

            ShootBall();
        }
    }

    private void SetIdleAnim()
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
        }
    }

    public void ResetTurn()
    {
        ballsShotThisTurn = 0;
        canMove = true;
        canShoot = true;

        // Update ball count from buffs
        UpdateBallCount();
    }
    
    public void UpdateBallCount()
    {
        // Apply ball count bonus from buffs
        if (BuffManager.Instance != null)
        {
            ballsPerTurn = 2 + BuffManager.Instance.ballCountBonus;
        }
        else
        {
            ballsPerTurn = 2;
        }

        ClampStuckPenalty();
        RecalculateCurrentTurnMaxShots();
    }

    public void AddHealth(float amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
    }

    public void AddMaxHealth(float amount)
    {
        maxHP += amount;
        currentHP += amount;
    }
    
    public void TakeDamageWithoutKnockback(float damage)
    {
        currentHP -= damage;
        
        // Play hit flash effect (no knockback)
        StartCoroutine(HitFlashCoroutine());
        
        if (currentHP <= 0)
        {
            Die();
        }
    }

    void ShootBall()
    {
        if (pinBallPrefab == null)
        {
            Debug.LogWarning("[pinBallPrefab] not assigned");
            return;
        }

        // mouse screen position to world position
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // get direction from player to mouse
        Vector2 dir = (mouseWorldPos - transform.position).normalized;

        // Determine direction based on shooting direction
        DetermineDirection(dir);

        // Play attack animation
        StartCoroutine(PlayAttackAnimation());

        // generate a ball at the player's position
        GameObject ball = Instantiate(pinBallPrefab, transform.position, Quaternion.identity);

        // Apply damage bonus from buffs (add to prefab's base damage)
        PinBall pinBall = ball.GetComponent<PinBall>();
        if (pinBall != null && BuffManager.Instance != null)
        {
            // Add damage bonus to the prefab's base damage value
            pinBall.damage += BuffManager.Instance.ballDamageBonus;
            pinBall.maxCollisions += BuffManager.Instance.ballCollisionBonus;
        }

        // initial velocity
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        if (ballRb != null)
        {
            ballRb.velocity = dir * ballSpeed;
        }
        else
        {
            Debug.LogWarning("[Rigidbody2D] component not found on [pinBallPrefab]");
        }

        ballsShotThisTurn++;

        // Disable movement and shooting after shooting all balls
        if (ballsShotThisTurn >= currentTurnMaxShots)
        {
            canMove = false;
            canShoot = false;
        }
    }

    public bool AreAllBallsShot()
    {
        return ballsShotThisTurn >= currentTurnMaxShots;
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
        
        // Stop movement immediately when paused
        if (paused && rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
    
    public int GetRemainingBalls()
    {
        return Mathf.Max(0, currentTurnMaxShots - ballsShotThisTurn);
    }

    // For UI display: show remaining balls ignoring penalties (only subtract actual shots)
    public int GetDisplayedBallCount()
    {
        return GetRemainingBalls();
    }

    public void ApplyBallSpeedMultiplier(float multiplier)
    {
        ballSpeed *= multiplier;
        if (trajectoryPredictor != null)
        {
            trajectoryPredictor.SetBallSpeed(ballSpeed);
        }
    }

    // Add penalty when balls are stuck on ice blocks
    public void AddStuckBallPenalty(int count)
    {
        stuckBallPenalty = Mathf.Max(0, stuckBallPenalty + count);
        ClampStuckPenalty();
    }

    // Remove penalty when ice blocks release balls
    public void RemoveStuckBallPenalty(int count)
    {
        stuckBallPenalty = Mathf.Max(0, stuckBallPenalty - count);
    }

    private void ClampStuckPenalty()
    {
        if (stuckBallPenalty > ballsPerTurn)
        {
            stuckBallPenalty = ballsPerTurn;
        }
    }

    private void RecalculateCurrentTurnMaxShots()
    {
        currentTurnMaxShots = Mathf.Max(0, ballsPerTurn - stuckBallPenalty);
        ballsShotThisTurn = Mathf.Min(ballsShotThisTurn, currentTurnMaxShots);
    }

    private void DetermineDirection(Vector2 direction)
    {
        // Determine which direction is dominant
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        if (absY > absX)
        {
            // Vertical direction
            currentDirection = direction.y > 0 ? 0 : 1; // 0=Up, 1=Down
        }
        else
        {
            // Horizontal direction
            currentDirection = direction.x > 0 ? 3 : 2; // 3=Right, 2=Left
        }
    }

    private IEnumerator PlayAttackAnimation()
    {
        if (animator != null)
        {
            // Get attack trigger name based on current direction
            string triggerName = GetAttackTriggerName();
            animator.SetTrigger(triggerName);
        }
        yield return new WaitForSeconds(attackAnimationDuration);
    }

    private string GetAttackTriggerName()
    {
        switch (currentDirection)
        {
            case 0: // Up
                return "AttackUp";
            case 1: // Down
                return "AttackDown";
            case 2: // Left
                return "AttackLeft";
            case 3: // Right
                return "AttackRight";
            default:
                return attackTriggerName;
        }
    }

    public void TakeKnockback(float distance, Transform bottomBoundary)
    {
        if (isBeingKnockedBack) return;
        StartCoroutine(KnockbackCoroutine(distance, bottomBoundary));
    }

    private IEnumerator KnockbackCoroutine(float distance, Transform bottomBoundary)
    {
        isBeingKnockedBack = true;

        // Stop Rigidbody2D velocity during knockback
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.down * distance;

        // Start hit flash effect
        StartCoroutine(HitFlashCoroutine());

        // Fast knockback movement
        float elapsed = 0f;
        float duration = distance / knockbackSpeed;

        // Ensure minimum duration to make it visible
        if (duration < 0.1f)
        {
            duration = 0.1f;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Use smooth step for better visual effect
            float smoothT = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPos, targetPos, smoothT);

            // Keep velocity zero during knockback
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            // Check if player touches bottom boundary during knockback
            if (bottomBoundary != null)
            {
                Collider2D playerCollider = GetComponent<Collider2D>();
                if (playerCollider != null)
                {
                    Collider2D boundaryCollider = bottomBoundary.GetComponent<Collider2D>();
                    if (boundaryCollider != null && playerCollider.IsTouching(boundaryCollider))
                    {
                        // Player touched bottom boundary, die immediately
                        isBeingKnockedBack = false;
                        Die();
                        yield break;
                    }
                }
                // Fallback: check position if no collider on boundary
                else if (transform.position.y <= bottomBoundary.position.y)
                {
                    isBeingKnockedBack = false;
                    Die();
                    yield break;
                }
            }

            yield return null;
        }

        transform.position = targetPos;
        isBeingKnockedBack = false;
    }

    private IEnumerator HitFlashCoroutine()
    {
        if (spriteRenderer == null) yield break;

        for (int i = 0; i < hitFlashCount; i++)
        {
            // Flash: make sprite completely invisible
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            yield return new WaitForSeconds(hitFlashDuration);

            // Return to original color (visible)
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(hitFlashDuration);
        }
    }

    private bool IsPointerOverUI()
    {
        // Check if mouse is over UI element
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private void UpdateTrajectory()
    {
        if (trajectoryPredictor == null) return;

        // Get mouse position in world space
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // Calculate direction from player to mouse
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        // Show trajectory
        trajectoryPredictor.ShowTrajectory(transform.position, direction);
    }

    private void HideTrajectory()
    {
        if (trajectoryPredictor != null)
        {
            trajectoryPredictor.HideTrajectory();
        }
    }

    public void Die()
    {
        if (isDead) return; // Prevent multiple death calls
        
        isDead = true;
        
        // Disable player control
        canMove = false;
        canShoot = false;
        
        // Start death effect coroutine
        StartCoroutine(DeathEffectCoroutine());
    }
    
    private IEnumerator DeathEffectCoroutine()
    {
        // Disable collision
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }
        
        // Disable Rigidbody2D physics
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true; // Make it kinematic so it doesn't interact with physics
        }
        
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
        
        // After death effect, return to main menu
        if (LevelManager.Instance != null)
        {
            // Stop timer if exists
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.StopTimer();
            }
            
            // Return to main menu
            LevelManager.Instance.ReturnToMainMenu();
        }
        else
        {
            // Fallback: directly load StartMenu scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("StartMenu");
        }
    }
}
