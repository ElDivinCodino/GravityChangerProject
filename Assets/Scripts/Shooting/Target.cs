﻿using UnityEngine;

public class Target : MonoBehaviour
{
    public float health;

    public void TakeDamage(float amount)
    {
        health -= amount;

        if (health <= 0f)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
