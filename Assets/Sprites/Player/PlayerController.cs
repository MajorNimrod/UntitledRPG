using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;

    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.4f;

    [Header("Knockback")]
    public float knockbackDecay = 16f; // how quickly it fades

    Rigidbody2D rb;
    Vector2 input;
    Vector2 knockbackVel;   // additive velocity from hits
    bool isDashing;
    float dashTimer;
    float dashCDTimer;
    Vector2 dashDir;
    bool inWater;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        // Gather input (old input system; swap for Input System if needed)
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        // normalized so diagonal movement isn't faster than straight.
        if (input.sqrMagnitude > 1f) input.Normalize();

        // Dash input
        dashCDTimer -= Time.deltaTime;
        if (!isDashing && dashCDTimer <= 0f && Input.GetKeyDown(KeyCode.Space) && input.sqrMagnitude > 0f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCDTimer = dashCooldown + dashDuration;
            dashDir = input;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Water")) inWater = true;
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Water")) inWater = false;
    }

    void FixedUpdate()
    {
        // Core move vector
        float slow = inWater ? 0.5f : 1f; // tune
        Vector2 desired = input * (moveSpeed * slow);

        // Dash override
        if (isDashing)
        {
            desired = dashDir * dashSpeed;
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f) isDashing = false;
        }


        // TODO: add knockback after enemies.
        // Apply decaying knockback (separate from core control)
        if (knockbackVel.sqrMagnitude > 0.0001f)
        {
            // Exponential decay toward zero
            knockbackVel = Vector2.MoveTowards(knockbackVel, Vector2.zero, knockbackDecay * Time.fixedDeltaTime);
        }

        // Final velocity to apply this tick
        Vector2 finalVel = desired + knockbackVel;

        // Use MovePosition for smooth, collision-friendly motion
        rb.MovePosition(rb.position + finalVel * Time.fixedDeltaTime);
    }

    // Call this from your combat/hit logic
    public void ApplyKnockback(Vector2 direction, float force)
    {
        // impulse toward direction (normalized)
        if (direction.sqrMagnitude > 0f)
            direction.Normalize();

        // Add to knockback velocity budget (doesn't fight movement)
        knockbackVel += direction * force;
    }
}