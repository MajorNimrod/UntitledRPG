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
    Vector2 lastNonZeroInput; // for idle facing direction
    static class AnimParams
    {
        public static readonly int MoveX = Animator.StringToHash("MoveX");
        public static readonly int MoveY = Animator.StringToHash("MoveY");
        public static readonly int IsMoving = Animator.StringToHash("IsMoving");
        public static readonly int IsRunning = Animator.StringToHash("IsRunning");
        public static readonly int Speed01 = Animator.StringToHash("Speed01");
        //Not implemented yet.
        //public static readonly int IsDashing = Animator.StringToHash("IsDashing");
        //public static readonly int InWater = Animator.StringToHash("InWater");
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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

        if (!isRunning && !isDashing && Input.GetKey(KeyCode.LeftShift) && input.sqrMagnitude > 0f)
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

    void UpdateAnimatorParams()
    {
        if (!animator) return;

        const float deadzone = 0.1f;
        bool hasMoveInput = input.sqrMagnitude > deadzone * deadzone;

        // Keep last facing for idle
        if (hasMoveInput) lastNonZeroInput = input;

        Vector2 animDir = isDashing
            ? (dashDir.sqrMagnitude > 0 ? dashDir : lastNonZeroInput)
            : (hasMoveInput ? input : lastNonZeroInput);

        float speed01 = isRunning ? 1f : 0f;    // 0=walk, 1=run
        if (isDashing) speed01 = 1f;            // optional: treat dash like "max speed"
        animator.SetFloat("Speed01", speed01);

        if (animDir.sqrMagnitude > 0f) animDir.Normalize();

        // --- smooth param writes ---
        const float dirDamp = 0.12f;    // ~120ms smoothing for direction
        const float speedDamp = 0.16f;  // ~160ms smoothing for walk<->run

        animator.SetFloat(AnimParams.MoveX, animDir.x, dirDamp, Time.deltaTime);
        animator.SetFloat(AnimParams.MoveY, animDir.y, dirDamp, Time.deltaTime);

        // Walk (0) vs Run (1). Treat dash like max speed if you want.
        float targetSpeed01 = (isDashing || (isRunning && hasMoveInput)) ? 1f : 0f;
        animator.SetFloat(AnimParams.Speed01, targetSpeed01, speedDamp, Time.deltaTime);

        // Keep movement tree active when moving or dashing
        animator.SetBool(AnimParams.IsMoving, hasMoveInput || isDashing);

        // Optional (Add later)
        ////animator.SetBool(AnimParams.IsDashing, isDashing);
        ////animator.SetBool(AnimParams.InWater, inWater);
    }

    void ValidateAnimatorParams()
    {
        if (!animator || animator.runtimeAnimatorController == null) return;

        // name -> required type
        var required = new (string name, AnimatorControllerParameterType type)[]
        {
        ("MoveX",    AnimatorControllerParameterType.Float),
        ("MoveY",    AnimatorControllerParameterType.Float),
        ("IsMoving", AnimatorControllerParameterType.Bool),
        //("IsDashing",AnimatorControllerParameterType.Bool),
        //("InWater",  AnimatorControllerParameterType.Bool),
        };

        var have = new Dictionary<string, AnimatorControllerParameterType>();
        foreach (var p in animator.parameters) have[p.name] = p.type;

        foreach (var (name, type) in required)
        {
            if (!have.TryGetValue(name, out var got))
                Debug.LogError($"Animator missing parameter '{name}' ({type}).");
            else if (got != type)
                Debug.LogError($"Animator parameter '{name}' has wrong type. Expected {type}, got {got}.");
        }
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