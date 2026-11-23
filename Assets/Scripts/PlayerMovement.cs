using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animator;
    public Transform groundCheck;
    public LayerMask groundLayer;

    private float horizontal;
    public float speed = 0.75f;
    public float walkSpeed = 0.75f;
    public float sprintSpeed = 1.5f;
    public float jumpingPower = 16f;
    public float radiusGC = 0.05f;
    private bool isFacingRight = true;
    private bool isAttacking = false;

    // --- Flip de animaciones ---
    // Asset con qué animaciones hay que voltear. Se edita con los botones del Inspector.
    public AnimationFlipSettings flipSettings;
    private SpriteRenderer spriteRenderer;

    // --- Long Idle ---
    // Segundos sin tocar los controles antes de pasar a Player_LongIdle
    public float longIdleDelay = 2f;
    // Cuánto tiempo lleva el jugador sin hacer nada
    private float idleTimer = 0f;



    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);

        if (!isFacingRight && horizontal > 0f)
        {
            Flip();
        }
        else if (isFacingRight && horizontal < 0f)
        {
            Flip();
        }

        CheckAttackFinished();
        UpdateIdleTimer();
        SetAnimation();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        ResetIdleTimer();

        // No se puede saltar mientras se ataca
        if (isAttacking) return;

        if(context.performed && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
        }

        if(context.canceled && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        ResetIdleTimer();

        // Mientras se ataca la velocidad debe seguir en 0
        if (isAttacking) return;

        if (context.performed && IsGrounded())
        {
            speed = sprintSpeed;
        }

        if (context.canceled)
        {
            speed = walkSpeed;
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        ResetIdleTimer();

        if (context.performed && IsGrounded())
        {
            speed = 0f;
            isAttacking = true;
        }

    }

    // Termina el ataque. Lo llama CheckAttackFinished() y también el
    // Animation Event del clip original (si existe). Llamarlo dos veces no da problemas.
    private void IsAttacking()
    {
        isAttacking = false;
        speed = walkSpeed;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, radiusGC, groundLayer);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    public void Move(InputAction.CallbackContext context)
    {
        ResetIdleTimer();
        horizontal = context.ReadValue<Vector2>().x;
    }

    // El contador solo avanza si el personaje está quieto, en el suelo y sin atacar.
    // En cualquier otro caso vuelve a 0.
    private void UpdateIdleTimer()
    {
        if (horizontal == 0 && IsGrounded() && !isAttacking)
        {
            idleTimer += Time.deltaTime;
        }
        else
        {
            ResetIdleTimer();
        }
    }

    // Se llama al pulsar o soltar cualquier control
    public void ResetIdleTimer()
    {
        idleTimer = 0f;
    }

    // Termina el ataque cuando el clip Player_Attack llega al final.
    // Así funciona con cualquier clip de ataque, aunque no tenga Animation Event.
    private void CheckAttackFinished()
    {
        if (!isAttacking) return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("Player_Attack") && state.normalizedTime >= 1f)
        {
            IsAttacking();
        }
    }

    // Reproduce un estado del Animator y aplica el flip configurado para él
    private void PlayAnimation(string stateName)
    {
        animator.Play(stateName);

        if (flipSettings != null && spriteRenderer != null)
        {
            spriteRenderer.flipX = flipSettings.IsFlipped(stateName);
        }
    }

    private void SetAnimation()
    {
        if (IsGrounded() && !isAttacking)
        {
            if (horizontal == 0)
            {
                if (idleTimer >= longIdleDelay)
                {
                    PlayAnimation("Player_LongIdle");
                }
                else
                {
                    PlayAnimation("Player_Idle");
                }
            }
            else if (speed <= walkSpeed)
            {
                PlayAnimation("Player_Walk");
            }
            else
                PlayAnimation("Player_Run");
        }
        else if(IsGrounded() && isAttacking)
        {
                PlayAnimation("Player_Attack");
        }
        else
        {
            if(rb.linearVelocityY > 0)
            {

                PlayAnimation("Player_Jump");
            }
            else
                PlayAnimation("Player_Fall");

        }
    }
}
