using UnityEngine;

public class Golem : Enemy
{
    protected override void Start()
    {
        maxHP = 25f;
        base.Start();
    }

    protected override float GetKnockbackDistance()
    {
        // Golem's knockback is twice the normal distance
        return (moveDistance + knockbackExtraDistance) * 2f;
    }
}

