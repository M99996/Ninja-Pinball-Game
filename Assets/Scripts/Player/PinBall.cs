using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinBall : MonoBehaviour
{
    [Header("Collision Settings")]
    public int maxCollisions = 10;
    public int damage = 1;
    public LayerMask enemyLayer;
    public float maxLifetime = 60f; // Maximum lifetime in seconds

    private int collisionCount = 0;
    private float lifetime = 0f;

    void Start()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.RegisterBall(this);
        }
    }

    void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.UnregisterBall(this);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject hitObject = collision.gameObject;

        // Check for IceBlock first
        IceBlock iceBlock = hitObject.GetComponent<IceBlock>();
        if (iceBlock != null)
        {
            // Deal damage first
            iceBlock.TakeDamage(damage);
            
            // Then stick the ball to the ice block
            iceBlock.StickBall(this);
            
            // Don't increment collision count or destroy ball here
            // Ball will be destroyed when ice block dies
            return;
        }

        if (IsInLayer(hitObject, enemyLayer))
        {
            Enemy enemy = hitObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Check if it's a ShieldGuard with front hit (shielded)
                ShieldGuard shieldGuard = enemy as ShieldGuard;
                if (shieldGuard != null)
                {
                    // ShieldGuard handles its own damage logic in OnCollisionEnter2D
                    // We skip damage here to let ShieldGuard decide
                    // ShieldGuard will call TakeDamage itself if it's a back hit
                }
                else
                { 
                    enemy.TakeDamage(damage);
                }
            }
        }

        collisionCount++;

        if (collisionCount >= maxCollisions)
        {
            Destroy(gameObject);
        }
    }

    private bool IsInLayer(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) != 0;
    }
}
