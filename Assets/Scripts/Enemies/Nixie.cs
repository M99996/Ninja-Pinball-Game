using System.Collections;
using UnityEngine;

public class Nixie : Enemy
{
    [Header("Nixie Special")]
    public GameObject jellyfishPrefab;
    [Range(0f, 1f)]
    public float spawnChance = 0.3f; // Chance to spawn
    public float spawnDistance = 1.5f; // Distance from Nixie to spawn position
    
    [Header("Spawn Animation")]
    public float spawnAnimationDuration = 1f; // Time for jellyfish to grow from small to normal size

    protected override void Start()
    {
        maxHP = 10f;
        base.Start();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        // Handle collision with PinBall (trigger collision)
        PinBall ball = other.GetComponent<PinBall>();
        if (ball != null)
        {
            TakeDamage(ball.damage);
        }
    }

    public override IEnumerator MoveDown()
    {
        // Call base MoveDown and wait for it to complete
        yield return StartCoroutine(base.MoveDown());
        
        // After movement completes, check if we should spawn a jellyfish
        // Only spawn if movement completed successfully
        if (!isMoving && canMove)
        {
            TrySpawnJellyfish();
        }
    }

    private void TrySpawnJellyfish()
    {
        if (Random.Range(0f, 1f) > spawnChance)
        {
            return;
        }

        if (jellyfishPrefab == null) return;

        // Get Nixie's collision bounds to calculate spawn positions
        Collider2D nixieCollider = GetComponent<Collider2D>();
        if (nixieCollider == null) return;

        Bounds nixieBounds = nixieCollider.bounds;
        
        // Get Jellyfish's actual collision size by instantiating a temporary one
        Vector3 jellyfishSize = GetJellyfishCollisionSize();
        if (jellyfishSize == Vector3.zero) return;
        
        float spawnOffset = nixieBounds.size.x * 0.5f + spawnDistance; // Half width + distance

        // Check left and right positions
        Vector3 leftPos = transform.position + Vector3.left * spawnOffset;
        Vector3 rightPos = transform.position + Vector3.right * spawnOffset;

        // Use Jellyfish's collision size to check if position is available
        bool leftAvailable = IsPositionAvailable(leftPos, jellyfishSize);
        bool rightAvailable = IsPositionAvailable(rightPos, jellyfishSize);

        // Determine spawn side (only if position is in camera view)
        Vector3? spawnPosition = null;
        if (leftAvailable && rightAvailable)
        {
            Vector3 chosenPos = Random.Range(0, 2) == 0 ? leftPos : rightPos;
            if (IsPositionInCameraView(chosenPos))
            {
                spawnPosition = chosenPos;
            }
        }
        else if (leftAvailable && IsPositionInCameraView(leftPos))
        {
            spawnPosition = leftPos;
        }
        else if (rightAvailable && IsPositionInCameraView(rightPos))
        {
            spawnPosition = rightPos;
        }

        // Spawn if position is available and in camera view
        if (spawnPosition.HasValue && jellyfishPrefab != null)
        {
            GameObject newJellyfish = Instantiate(jellyfishPrefab, spawnPosition.Value, Quaternion.identity);
            StartCoroutine(SpawnAnimationCoroutine(newJellyfish));
        }
    }

    private Vector3 GetJellyfishCollisionSize()
    {
        if (jellyfishPrefab == null) return Vector3.zero;
        
        Collider2D prefabCollider = jellyfishPrefab.GetComponent<Collider2D>();
        if (prefabCollider == null) return Vector3.zero;
        
        // For BoxCollider2D, use size directly
        BoxCollider2D boxCollider = prefabCollider as BoxCollider2D;
        if (boxCollider != null)
        {
            return boxCollider.size;
        }
        
        // For other collider types, instantiate temporarily to get actual bounds
        GameObject tempJellyfish = Instantiate(jellyfishPrefab, Vector3.zero, Quaternion.identity);
        tempJellyfish.SetActive(false);
        
        Collider2D tempCollider = tempJellyfish.GetComponent<Collider2D>();
        Vector3 size = Vector3.zero;
        
        if (tempCollider != null)
        {
            size = tempCollider.bounds.size;
        }
        
        Destroy(tempJellyfish);
        
        return size;
    }

    private bool IsPositionAvailable(Vector3 position, Vector3 size)
    {
        // Use Physics2D.OverlapBox to check if position is clear
        Collider2D[] overlaps = Physics2D.OverlapBoxAll(
            position,
            size,
            0f
        );

        // Filter out only the Nixie itself
        // Check all colliders (including triggers)
        foreach (Collider2D col in overlaps)
        {
            if (col != null && col.gameObject != gameObject)
            {
                return false; // Position is occupied
            }
        }

        return true; // Position is available
    }

    private bool IsPositionInCameraView(Vector3 position)
    {
        // Check if position is within camera view bounds
        if (Camera.main == null) return true; // If no camera, allow spawn
        
        // Convert world position to viewport coordinates (0-1 range)
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(position);
        
        // Check if position is within camera view (with some margin for edge cases)
        // x and y should be between 0 and 1, z should be positive (in front of camera)
        return viewportPos.x >= 0f && viewportPos.x <= 1f &&
               viewportPos.y >= 0f && viewportPos.y <= 1f &&
               viewportPos.z > 0f;
    }

    private IEnumerator SpawnAnimationCoroutine(GameObject jellyfish)
    {
        if (jellyfish == null) yield break;

        // Store original scale
        Vector3 originalScale = jellyfish.transform.localScale;
        
        // Start from very small
        jellyfish.transform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < spawnAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spawnAnimationDuration;
            
            // Smooth scale interpolation (ease out)
            float scaleValue = Mathf.SmoothStep(0f, 1f, t);
            jellyfish.transform.localScale = originalScale * scaleValue;

            yield return null;
        }

        // Ensure final scale is correct
        jellyfish.transform.localScale = originalScale;
    }
}

