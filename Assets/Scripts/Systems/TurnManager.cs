using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Turn Settings")]
    public Transform bottomBoundary;
    public float enemyMoveDuration = 0.5f;

    private List<PinBall> activeBalls = new List<PinBall>();
    private List<Fire> activeFires = new List<Fire>(); // Track all active fires
    private List<Skeleton> activeSkeletons = new List<Skeleton>(); // Track all skeletons in near-death state
    private bool isPlayerTurn = true;
    private bool isEnemyMoving = false;
    private int currentTurn = 0;
    private PlayerCharacter player;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        player = FindObjectOfType<PlayerCharacter>();
        
        // Start first turn
        currentTurn = 1;
        
        // Show buff selection for first turn
        // Use coroutine to ensure BuffManager is ready
        StartCoroutine(DelayedTurnStart());
    }
    
    private IEnumerator DelayedTurnStart()
    {
        // Wait one frame to ensure all Start() methods have executed
        yield return null;
        OnTurnStart();
    }


    public void RegisterBall(PinBall ball)
    {
        if (!activeBalls.Contains(ball))
        {
            activeBalls.Add(ball);
        }
    }

    public void UnregisterBall(PinBall ball)
    {
        activeBalls.Remove(ball);
        
        // Check if all conditions are met to end player turn
        // Only check when a ball is destroyed, not when registered
        CheckEndPlayerTurn();
    }

    public void CheckEndPlayerTurn()
    {
        if (!isPlayerTurn || isEnemyMoving) return;
        
        // Check if player has shot all balls
        bool allBallsShot = false;
        if (player != null)
        {
            allBallsShot = player.AreAllBallsShot();
        }
        
        // End turn only if: all balls are shot AND all balls are destroyed
        // This ensures we wait for all balls to be shot AND all to be destroyed
        // Important: Must check both conditions - if player hasn't shot all balls yet,
        // we should not end the turn even if activeBalls.Count == 0
        if (allBallsShot && activeBalls.Count == 0)
        {
            EndPlayerTurn();
        }
    }

    public void EndPlayerTurn()
    {
        if (!isPlayerTurn || isEnemyMoving) return;

        isPlayerTurn = false;
        StartCoroutine(EnemyTurnCoroutine());
    }

    private IEnumerator EnemyTurnCoroutine()
    {
        isEnemyMoving = true;

        // Get all enemies in scene
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        
        // Assign bottomBoundary to all enemies
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.bottomBoundary = bottomBoundary;
            }
        }
        
        // Check and start skeleton revivals (should happen at the same time as enemy movement)
        NotifySkeletonsForRevival();
        
        // Start all enemies moving simultaneously
        float maxMoveDuration = 0f;
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.canMove)
            {
                // Set move duration if not already set
                if (enemy.moveDuration <= 0)
                {
                    enemy.moveDuration = enemyMoveDuration;
                }
                StartCoroutine(enemy.MoveDown());
                maxMoveDuration = Mathf.Max(maxMoveDuration, enemy.moveDuration);
            }
        }

        // Wait for all enemies to finish moving
        yield return new WaitForSeconds(maxMoveDuration);

        // Small delay to ensure all collisions are processed
        yield return new WaitForSeconds(0.1f);

        isEnemyMoving = false;
        isPlayerTurn = true;

        // Wait 2 seconds before starting next turn
        yield return new WaitForSeconds(2f);

        // Increment turn counter
        currentTurn++;

        // Notify all fires that a turn has ended
        NotifyFiresTurnEnd();

        // Start new turn (buff selection will be added later)
        OnTurnStart();
    }

    public void HandleEnemyPlayerCollision(Enemy enemy, PlayerCharacter player, float knockbackDistance)
    {
        if (player == null) return;

        // Start knockback coroutine with hit flash effect
        player.TakeKnockback(knockbackDistance, bottomBoundary);
    }

    private void DestroyAllBalls()
    {
        List<PinBall> ballsToDestroy = new List<PinBall>(activeBalls);
        foreach (var ball in ballsToDestroy)
        {
            if (ball != null)
            {
                Destroy(ball.gameObject);
            }
        }
        activeBalls.Clear();
    }

    private void OnTurnStart()
    {
        // Show buff selection at the start of each turn
        if (BuffManager.Instance != null)
        {
            BuffManager.Instance.ShowBuffSelection();
        }
        
        if (player != null)
        {
            player.ResetTurn();
        }
    }

    public bool IsPlayerTurn()
    {
        return isPlayerTurn && !isEnemyMoving;
    }

    public bool CanShoot()
    {
        return isPlayerTurn && !isEnemyMoving;
    }

    // Register a fire to track turns
    public void RegisterFire(Fire fire)
    {
        if (fire != null && !activeFires.Contains(fire))
        {
            activeFires.Add(fire);
        }
    }

    // Unregister a fire
    public void UnregisterFire(Fire fire)
    {
        activeFires.Remove(fire);
    }

    // Notify all fires that a turn has ended
    private void NotifyFiresTurnEnd()
    {
        // Create a copy of the list to avoid modification during iteration
        List<Fire> firesToNotify = new List<Fire>(activeFires);
        foreach (Fire fire in firesToNotify)
        {
            if (fire != null)
            {
                fire.OnTurnEnd();
            }
        }
    }

    // Register a skeleton to track turns for resurrection
    public void RegisterSkeleton(Skeleton skeleton)
    {
        if (skeleton != null && !activeSkeletons.Contains(skeleton))
        {
            activeSkeletons.Add(skeleton);
        }
    }

    // Unregister a skeleton
    public void UnregisterSkeleton(Skeleton skeleton)
    {
        activeSkeletons.Remove(skeleton);
    }

    // Check and start skeleton revivals (called at the start of enemy turn, before enemies move)
    private void NotifySkeletonsForRevival()
    {
        // Create a copy of the list to avoid modification during iteration
        List<Skeleton> skeletonsToCheck = new List<Skeleton>(activeSkeletons);
        foreach (Skeleton skeleton in skeletonsToCheck)
        {
            if (skeleton != null)
            {
                skeleton.CheckAndStartRevival();
            }
        }
    }

    // Get current turn number
    public int GetCurrentTurn()
    {
        return currentTurn;
    }
}

