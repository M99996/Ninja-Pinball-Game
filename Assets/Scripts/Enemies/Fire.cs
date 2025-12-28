using UnityEngine;

public class Fire : MonoBehaviour
{
    [Header("Fire Settings")]
    public int damage = 2; // Damage dealt to player
    public int durationTurns = 3; // How many turns the fire lasts
    public float damageCooldown = 0.5f; // Cooldown between damage ticks (seconds)
    
    private int remainingTurns;
    private Collider2D fireCollider;
    private SpriteRenderer fireRenderer;
    private float lastDamageTime = 0f; // Track when last damage was dealt

    void Start()
    {
        remainingTurns = durationTurns;
        fireCollider = GetComponent<Collider2D>();
        fireRenderer = GetComponent<SpriteRenderer>();
        
        // Register to TurnManager to receive turn notifications
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.RegisterFire(this);
        }
    }

    void OnDestroy()
    {
        // Unregister from TurnManager
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.UnregisterFire(this);
        }
    }

    // Called by TurnManager when a turn ends
    public void OnTurnEnd()
    {
        remainingTurns--;
        
        if (remainingTurns <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player entered the fire
        PlayerCharacter player = other.GetComponent<PlayerCharacter>();
        if (player != null && !player.IsDead)
        {
            // Deal damage immediately when entering
            player.TakeDamageWithoutKnockback(damage);
            lastDamageTime = Time.time;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Check if player is staying in fire (with cooldown to avoid damage spam)
        PlayerCharacter player = other.GetComponent<PlayerCharacter>();
        if (player != null && !player.IsDead)
        {
            // Only deal damage if cooldown has passed
            if (Time.time - lastDamageTime >= damageCooldown)
            {
                player.TakeDamageWithoutKnockback(damage);
                lastDamageTime = Time.time;
            }
        }
    }
}