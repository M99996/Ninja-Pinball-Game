using UnityEngine;

public class ShieldGuard : Enemy
{
    [Header("Shield Guard Special")]
    [Range(-1f, 1f)]
    public float frontThreshold = 0.5f;

    protected override void Start()
    {
        maxHP = 20f;
        base.Start();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        PinBall ball = collision.gameObject.GetComponent<PinBall>();
        if (ball == null) return;

        bool isFrontHit = IsFrontHit(collision);

        if (!isFrontHit)
        {
            TakeDamage(ball.damage);
        }

    }
    private bool IsFrontHit(Collision2D collision)
    {
        if (collision.contactCount == 0) return false;

        Vector2 facingDirection = Vector2.down;

        Vector2 contactPoint = collision.GetContact(0).point;
        Vector2 shieldPosition = transform.position;
        Vector2 hitDirection = (contactPoint - shieldPosition).normalized;

        float dot = Vector2.Dot(hitDirection, facingDirection);

        return dot >= frontThreshold;
    }
}