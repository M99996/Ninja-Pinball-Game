using UnityEngine;

public class FireElemental : Enemy
{
    [Header("Fire Elemental Special")]
    public GameObject firePrefab;

    protected override void Start()
    {
        maxHP = 15f;   
        base.Start();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PinBall ball = other.GetComponent<PinBall>();
        if (ball != null)
        {
            TakeDamage(ball.damage);
        }
    }

    public override void Die()
    {
        // Spawn fire at death position before calling base Die
        if (firePrefab != null)
        {
            Instantiate(firePrefab, transform.position, Quaternion.identity);
        }
        
        // Call base Die to handle death animation
        base.Die();
    }
}

