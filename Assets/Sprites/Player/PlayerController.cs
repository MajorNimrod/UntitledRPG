using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;
    public float runSpeedMultiplier = 1.5f;

    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.4f;

    [Header("Knockback")]
    public float knockbackDecay = 16f; // how quickly it fades

    Rigidbody2D rb;
    Vector2 input;
    Vector2 knockbackVel;   // additive velocity from hits
    bool isRunning;
    bool isDashing;
    float dashTimer;
    float dashCDTimer;
    Vector2 dashDir;
    bool inWater;

    [Header("Animation")]
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    Vector2 lastNonZeroInput; // for idle facing direction
    static class AnimParams
    {
        public static readonly int IsMoving  = Animator.StringToHash("IsMoving");
        public static readonly int IsRunning = Animator.StringToHash("IsRunning");
        public static readonly int Speed     = Animator.StringToHash("Speed");
        //Not implemented yet.
        //public static readonly int IsDashing = Animator.StringToHash("IsDashing");
        //public static readonly int InWater = Animator.StringToHash("InWater");
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (!animator) Debug.LogWarning("Animator missing on Player.");
        if (!spriteRenderer) Debug.LogWarning("SpriteRenderer missing on Player.");

        ValidateAnimatorParams();
    }

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
            BeginDash();
        }

        if (!isDashing && Input.GetKey(KeyCode.LeftShift) && input.sqrMagnitude > 0f)
        {
            isRunning = true;
        }

        // last facing direction tracking
        if (input.sqrMagnitude > 0.0001f)
        {
            lastNonZeroInput = input;
        }

        UpdateAnimatorParams();
    }

    private void LateUpdate()
    {
        HandleSpriteFlip();
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

        // Apply run multiplier for this tick, then clear flag (re-evaluated each Update)
        if (isRunning && !isDashing)
        {
            desired *= runSpeedMultiplier;
            isRunning = false;
        }


        // TODO: add knockback after enemies.
        // Apply decaying knockback (separate from core control)
        if (knockbackVel.sqrMagnitude > 0.0001f)
        {
            // Exponential decay toward zero
            knockbackVel = Vector2.MoveTowards(knockbackVel, Vector2.zero, knockbackDecay * Time.fixedDeltaTime);
        }

        // Final movement
        Vector2 finalVel = desired + knockbackVel;
        rb.MovePosition(rb.position + finalVel * Time.fixedDeltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Water")) inWater = true;
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Water")) inWater = false;
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

    void UpdateAnimatorParams()
    {
        if (!animator) return;

        const float deadzone = 0.1f;
        bool hasMoveInput = input.sqrMagnitude > deadzone * deadzone;

        // 0 = walk, 1 = run/dash
        float targetSpeed01 = (isDashing || (hasMoveInput && Input.GetKey(KeyCode.LeftShift))) ? 1f : 0f;

        const float speedDamp = 0.16f; // smooth the walk<->run blend
        animator.SetFloat(AnimParams.Speed, targetSpeed01, speedDamp, Time.deltaTime);

        // Keep the move state active whenever there’s input or we’re dashing
        animator.SetBool(AnimParams.IsMoving, hasMoveInput || isDashing);
    }

    void HandleSpriteFlip()
    {
        if (!spriteRenderer) return;

        // Choose facing: dash > input > last known
        Vector2 dir = isDashing ? dashDir :
                      (input.sqrMagnitude > 0.01f ? input : lastNonZeroInput);

        if (dir.x < -0.01f) spriteRenderer.flipX = true;   // face left
        else if (dir.x > 0.01f) spriteRenderer.flipX = false;  // face right
        // if ~0, keep current facing
    }

    void ValidateAnimatorParams()
    {
        if (!animator || animator.runtimeAnimatorController == null) return;

        var have = new Dictionary<string, AnimatorControllerParameterType>();
        foreach (var p in animator.parameters) have[p.name] = p.type;

        if (!have.TryGetValue("Speed", out var tS) || tS != AnimatorControllerParameterType.Float)
            Debug.LogWarning("Animator: expected float parameter 'Speed' for the 1D blend tree.");

        if (!have.TryGetValue("IsMoving", out var tM) || tM != AnimatorControllerParameterType.Bool)
            Debug.LogWarning("Animator: expected bool parameter 'IsMoving' for Idle<->Move transitions.");
    }

    void BeginDash()
    {
        // abstracted method in order to avoid mutating the dashDir vector.
        isDashing = true;
        dashTimer = dashDuration;
        dashCDTimer = dashCooldown + dashDuration;

        var dir = (input.sqrMagnitude > 0f) ? input : lastNonZeroInput;
        dashDir = (dir.sqrMagnitude > 0f) ? dir.normalized : Vector2.right;
    }
}