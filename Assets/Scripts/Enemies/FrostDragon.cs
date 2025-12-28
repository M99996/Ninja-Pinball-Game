using System.Collections;
using UnityEngine;

public class FrostDragon : Enemy
{
    [Header("Frost Dragon Special")]
    public GameObject iceBlockPrefab;
    public float minSpawnHeightAboveBoundary = 3f; // Minimum distance above bottom boundary
    public float spawnAnimationDuration = 1f;
    
    [Header("Camera Shake")]
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 0.2f;

    protected override void Start()
    {
        maxHP = 50f;
        base.Start();
    }

    public override IEnumerator MoveDown()
    {
        // Start camera shake
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);
        }
        
        // Call base movement
        yield return StartCoroutine(base.MoveDown());
        
        // After moving, spawn ice block
        SpawnIceBlock();
    }

    private void SpawnIceBlock()
    {
        if (iceBlockPrefab == null || bottomBoundary == null)
        {
            return;
        }

        // Determine spawn range
        float minY = bottomBoundary.position.y + minSpawnHeightAboveBoundary;
        float maxY = minY + 5f; // Default range if camera not found
        float minX = -5f;
        float maxX = 5f;
        
        if (Camera.main != null)
        {
            Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
            Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.nearClipPlane));
            minX = bottomLeft.x;
            maxX = topRight.x;
            maxY = topRight.y;
        }
        
        // Ensure maxY is above minY
        if (maxY <= minY)
        {
            maxY = minY + 0.5f;
        }
        
        const int maxAttempts = 5;
        bool spawned = false;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);
            Vector3 spawnPosition = new Vector3(randomX, randomY, 0f);

            if (IsPositionAvailable(spawnPosition))
            {
                GameObject newIceBlock = Instantiate(iceBlockPrefab, spawnPosition, Quaternion.identity);
                StartCoroutine(SpawnAnimationCoroutine(newIceBlock));
                spawned = true;
                break;
            }
        }
    }

    private bool IsPositionAvailable(Vector3 position)
    {
        // Check for colliders at this position
        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            position,
            GetIceBlockSize(),
            0f
        );
        
        // Filter out triggers and this enemy itself
        foreach (Collider2D col in colliders)
        {
            if (col == null || col.isTrigger) continue;

            // Ignore bottom boundary collider so we can place ice blocks slightly above it
            if (bottomBoundary != null && col.gameObject == bottomBoundary.gameObject) continue;

            // Prevent overlapping other solid objects
            if (col.gameObject != gameObject)
            {
                return false;
            }
        }
        
        return true;
    }

    private Vector3 GetIceBlockSize()
    {
        if (iceBlockPrefab == null) return Vector3.one;
        
        Collider2D prefabCollider = iceBlockPrefab.GetComponent<Collider2D>();
        if (prefabCollider == null) return Vector3.one;
        
        BoxCollider2D boxCollider = prefabCollider as BoxCollider2D;
        if (boxCollider != null)
        {
            return boxCollider.size;
        }
        
        // Fallback: instantiate temporarily to get bounds
        GameObject temp = Instantiate(iceBlockPrefab, Vector3.zero, Quaternion.identity);
        temp.SetActive(false);
        
        Collider2D tempCollider = temp.GetComponent<Collider2D>();
        Vector3 size = Vector3.one;
        
        if (tempCollider != null)
        {
            size = tempCollider.bounds.size;
        }
        
        Destroy(temp);
        return size;
    }

    private IEnumerator SpawnAnimationCoroutine(GameObject iceBlock)
    {
        if (iceBlock == null) yield break;
        
        Vector3 originalScale = iceBlock.transform.localScale;
        iceBlock.transform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        while (elapsed < spawnAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spawnAnimationDuration;
            
            // Smooth scale animation
            iceBlock.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            
            yield return null;
        }
        
        iceBlock.transform.localScale = originalScale;
    }
}

