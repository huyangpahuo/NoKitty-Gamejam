using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("跳跃")]
    [SerializeField] private float jumpForce = 8f;

    [Header("地面检测")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("落地动画")]
    [Tooltip("空中停留多久才触发Land动画")]
    [SerializeField] private float minFallTime = 0.2f;

    [Header("行走粒子")]
    [SerializeField] private ParticleSystem walkParticle;

    private Rigidbody2D rb;
    private Animator animator;

    private float moveInput;

    private bool isGrounded;
    private bool wasGrounded;

    private float airTime;

    // -1 = Left
    //  1 = Right
    private int direction = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        MoveInput();

        CheckGround();

        UpdateAirTime();

        CheckLanding();

        UpdateAnimator();

        UpdateWalkParticle();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        Move();
    }


    private void MoveInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0)
        {
            direction = 1;
        }
        else if (moveInput < 0)
        {
            direction = -1;
        }
    }

    private void Move()
    {
        rb.velocity = new Vector2(
            moveInput * moveSpeed,
            rb.velocity.y
        );
    }

    private void Jump()
    {
        rb.velocity = new Vector2(
            rb.velocity.x,
            jumpForce
        );

        AudioManager.Instance.PlaySFX("JumpClip");
    }

    private void UpdateAnimator()
    {
        animator.SetFloat(
            "Speed",
            Mathf.Abs(moveInput)
        );

        animator.SetInteger(
            "Direction",
            direction
        );
    }

    private void UpdateAirTime()
    {
        if (!isGrounded)
        {
            airTime += Time.deltaTime;
        }
    }

    private void CheckLanding()
    {
        // 落地瞬间
        if (!wasGrounded && isGrounded)
        {
            // 只有真正下落一段时间才触发
            if (airTime >= minFallTime)
            {
                animator.SetTrigger("Land");
            }

            airTime = 0f;
        }

        wasGrounded = isGrounded;
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            checkRadius,
            groundLayer
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            checkRadius
        );
    }

    private void UpdateWalkParticle()
    {
        bool shouldPlay =
            isGrounded &&
            Mathf.Abs(moveInput) > 0.1f;

        if (shouldPlay)
        {
            if (!walkParticle.isPlaying)
            {
                walkParticle.Play();
            }
        }
        else
        {
            if (walkParticle.isPlaying)
            {
                walkParticle.Stop();
            }
        }
    }
}